using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.CR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.CR.Request;
using ThirdPartyWallet.Share.Model.Game.CR.Response;

namespace H1_ThirdPartyWalletAPI.Worker.Game.CR
{
    public class CRRecordSchedule : IInvocable
    {
        private readonly ILogger<CRRecordSchedule> _logger;
        private readonly ICRApiService _CRApiService;
        private readonly ICRInterfaceService _apiInterfaceService;
        private readonly ICacheDataService _cacheDataService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        public const string SYSTEM_PARAMETERS_KEY = "CRRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public CRRecordSchedule(ILogger<CRRecordSchedule> logger, ICRInterfaceService apiInterfaceService, ICRApiService CRApiService, ISystemParameterDbService systemParameterDbService, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _CRApiService = CRApiService;
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
            _logger.LogInformation("Invoke CRRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());

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
                    name = "CR取得注單排程",
                    description = "CR記錄EndTime"
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
                _logger.LogInformation("CR record stop time: {time}", parameter.value);
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
                var pageIndex = 1;
                // 取得總頁數
                int? pageCount = null;
                //// 每頁取得筆數
                //var pageLimit = 50;
                // 取得注單delay時間(毫秒)
                int callAPIdelayMS = 1000;


                //每頁50筆
                var recordRequest = new ALLWagerRequest
                {
                    dateStart = lastEndTime.AddHours(-12),
                    dateEnd = nextTime.AddHours(-12),
                    settle = 1,
                    langx = "en-us",
                    page = pageIndex
                };

                // 如果已經慢一小時以上，每次從廠商那邊取得一小時內的資料
                if ((nextTime - lastEndTime).TotalMinutes > 60)
                {
                    recordRequest.dateEnd = lastEndTime.AddHours(1).AddMilliseconds(-1);
                    parameter.value = lastEndTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                    RANGE_OFFSET = lastEndTime.AddHours(1) - lastEndTime;
                }
                await _systemParameterDbService.PutSystemParameter(parameter);

                //從CR取得的原始資料
                List<Wager_Data> gameProviderBetRecords = new List<Wager_Data>();
                //要寫入的資料
                List<Wager_Data> postBetRecords = new List<Wager_Data>();
                var resPK = gameProviderBetRecords.Select(x => new { x.id, x.cashoutid, x.adddate }).ToHashSet();

                do
                {
                    // 設定頁碼
                    recordRequest.page = pageIndex;

                    var betRecord = await _CRApiService.ALLWagerAsync(recordRequest, 2);
                    if (betRecord.wager_data != null && betRecord.wager_data.Count > 0)
                    {
                        foreach (var item in betRecord.wager_data)
                        {
                            if (!item.resultdate.HasValue)
                            {
                                item.resultdate = item.adddate;
                            }
                            item.adddate = item.adddate.AddHours(12);
                            item.resultdate = item.resultdate.Value.AddHours(12);

                            if (resPK.Add(new { item.id, item.cashoutid, item.adddate }))
                            {
                                gameProviderBetRecords.Add(item);
                            }
                        }
                    }


                    pageCount = betRecord.wager_totalpage;

                    pageIndex++;
                    await Task.Delay(callAPIdelayMS); // (查詢注單限制，如果有在設定時間)
                } while (pageCount > pageIndex);


                //同時段條件讀取未結算單
                recordRequest.settle = 0;
                pageIndex = 1;

                do
                {
                    // 設定頁碼
                    recordRequest.page = pageIndex;
                    var betRecord = await _CRApiService.ALLWagerAsync(recordRequest, 2);
                    if (betRecord.wager_data != null && betRecord.wager_data.Count > 0)
                    {
                        foreach (var item in betRecord.wager_data)
                        {
                            if (!item.resultdate.HasValue)
                            {
                                item.resultdate = item.adddate;
                            }
                            item.adddate = item.adddate.AddHours(12);
                            item.resultdate = item.resultdate.Value.AddHours(12);

                            if (resPK.Add(new { item.id, item.cashoutid, item.adddate }))
                            {
                                gameProviderBetRecords.Add(item);
                            }
                        }
                    }

                    pageCount = betRecord.wager_totalpage;

                    pageIndex++;
                    await Task.Delay(callAPIdelayMS); // (查詢注單限制，如果有在設定時間)
                } while (pageCount > pageIndex);

                if (gameProviderBetRecords.Any() == true)
                {
                    // 排除重複注單
                    postBetRecords = gameProviderBetRecords.DistinctBy(record => new { record.id, record.cashoutid, record.result, record.adddate }).ToList();

                    if (postBetRecords.Any() == true)
                    {
                        await _apiInterfaceService.PostCRRecord(postBetRecords);
                    }
                }
            }
            catch (Exception ex)
            {
                TriggerFailOver(parameter, RANGE_OFFSET);
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run CR record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        private async void TriggerFailOver(t_system_parameter parameter, TimeSpan offTimeSpan)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            Platform PlatformCR = Platform.CR;

            var failoverReq = new PullRecordFailoverWithTimeOffset()
            {
                platform = PlatformCR,
                repairParameter = parameter.value, // 已經是新的時間
                delay = TimeSpan.FromMinutes(5),
                OffTimeSpan = -offTimeSpan         // 轉換成負值，是要找回起始時間
            };

            await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{PlatformCR}", failoverReq);
        }
    }
}