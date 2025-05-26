using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using H1_ThirdPartyWalletAPI.Service.Game.RGRICH;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Request;
using System.Linq;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Response;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Response;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Enum;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Worker.Game.RGRICH
{
    public class RGRICHRecordSchedule : IInvocable
    {
        private readonly ILogger<RGRICHRecordSchedule> _logger;
        private readonly IRGRICHApiService _RGRICHApiService;
        private readonly IRGRICHInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ICacheDataService _cacheDataService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        public const string SYSTEM_PARAMETERS_KEY = "RGRICHRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public RGRICHRecordSchedule(ILogger<RGRICHRecordSchedule> logger, IRGRICHInterfaceService apiInterfaceService, IRGRICHApiService RGRICHApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _RGRICHApiService = RGRICHApiService;
            _dbService = dbService;
            _apiInterfaceService = apiInterfaceService;
            _systemParameterDbService = systemParameterDbService;
            _cacheDataService = cacheDataService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke RGRICHRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());

            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.Add(GAP_TIME);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            // 取得上次結束時間
            var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

            // 檢查有無資料，沒資料的話新增預設值
            if (parameter == null)
            {
                var model = new t_system_parameter()
                {
                    key = SYSTEM_PARAMETERS_KEY,
                    value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    name = "RGRICH取得注單排程",
                    description = "RGRICH記錄end_time"
                };

                var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                if (postSystemParameter)
                {
                    parameter = model;
                }
                else
                {
                    return; // 新增失敗就結束排程
                }
            }

            if (int.Parse(parameter.min_value) == 0)
            {
                _logger.LogInformation("RGRICH record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");
            RANGE_OFFSET = nextTime - lastEndTime;

            if (lastEndTime >= nextTime)
            {
                return;
            }
            try
            {
                // 當前頁碼
                var pageIndex = 0;
                // 取得總頁數
                int? pageCount = null; // = (responseData.Data.DataCount / pageLimit) + 1;
                // 每頁取得筆數
                var pageLimit = 5000;
                // 取得注單delay時間(毫秒)
                int callAPIdelayMS = 1000;

                var recordRequest = new BetRecordRequest
                {
                    SearchMode = SearchMode.UpdatedAt,
                    PerPage = pageLimit,
                    StartTime = lastEndTime,
                    EndTime = nextTime.AddMilliseconds(-1),
                    // StartTime = parameter.value
                };

                // 如果已經慢一小時以上，每次從廠商那邊取得一小時內的資料
                if ((nextTime - lastEndTime).TotalMinutes > 60)
                {
                    recordRequest.EndTime = lastEndTime.AddHours(1).AddMilliseconds(-1);
                    parameter.value = lastEndTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                    RANGE_OFFSET = lastEndTime.AddHours(1) - lastEndTime;
                }
                await _systemParameterDbService.PutSystemParameter(parameter);

                //從RG富遊取得的原始資料
                List<BetRecordResponse> gameProviderBetRecords = new List<BetRecordResponse>();
                //要寫入的資料
                List<BetRecordResponse> postBetRecords = new List<BetRecordResponse>();

                /*
                 // 遊戲商回傳資料格式
                 "meta": {
                   "last_page": 1,
                   "per_page": 1000,
                   "page": 1,
                   "total": 949
                   }
                 */
                do
                {
                    // 設定頁碼
                    recordRequest.Page = pageIndex;

                    var betRecord = await _RGRICHApiService.BetRecordAsync(recordRequest);
                    // 有錯誤就拋
                    if (string.IsNullOrEmpty(betRecord.Message) == false && betRecord.Success == false)
                    {
                        throw new Exception(betRecord.Message);
                    }

                    // 加入遊戲商回應的注單集合
                    gameProviderBetRecords.AddRange(betRecord.Data ?? new List<BetRecordResponse>());

                    if (pageCount.HasValue == false)
                    {
                        pageCount = (betRecord.Meta?.Total / pageLimit) + 1 ?? 0;
                    }

                    pageIndex++;
                    await Task.Delay(callAPIdelayMS); // (查詢注單限制，如果有在設定時間)
                } while (pageCount > pageIndex);

                if (gameProviderBetRecords.Any() == true)
                {
                    // 排除重複注單
                    postBetRecords = gameProviderBetRecords.DistinctBy(record => new { record.Bet_no, record.Status, record.Bet_time }).ToList();

                    if (postBetRecords.Any() == true)
                    {
                        await _apiInterfaceService.PostRGRICHRecord(postBetRecords);
                    }
                }
            }
            catch (Exception ex)
            {
                TriggerFailOver(parameter, RANGE_OFFSET);
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run RGRICH record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        private async void TriggerFailOver(t_system_parameter parameter, TimeSpan offTimeSpan)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            Platform PlatformRGRICH = Platform.RGRICH;

            var failoverReq = new PullRecordFailoverWithTimeOffset()
            {
                platform = PlatformRGRICH,
                repairParameter = parameter.value, // 已經是新的時間
                delay = TimeSpan.FromMinutes(5),
                OffTimeSpan = -offTimeSpan         // 轉換成負值，是要找回起始時間
            };

            await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{PlatformRGRICH}", failoverReq);
        }
    }
}