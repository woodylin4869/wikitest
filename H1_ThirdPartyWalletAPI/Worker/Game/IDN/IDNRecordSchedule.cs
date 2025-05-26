using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.IDN;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.IDN.Request;
using ThirdPartyWallet.Share.Model.Game.IDN.Response;

namespace H1_ThirdPartyWalletAPI.Worker.Game.IDN
{
    public class IDNRecordSchedule : IInvocable
    {
        private readonly ILogger<IDNRecordSchedule> _logger;
        private readonly IIDNApiService _IDNApiService;
        private readonly IIDNInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ICacheDataService _cacheDataService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        public const string SYSTEM_PARAMETERS_KEY = "IDNRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public IDNRecordSchedule(ILogger<IDNRecordSchedule> logger, IIDNInterfaceService apiInterfaceService, IIDNApiService IDNApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _IDNApiService = IDNApiService;
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
            _logger.LogInformation("Invoke IDNRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());

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
                    name = "IDN取得注單排程",
                    description = "IDN記錄end_time"
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
                _logger.LogInformation("IDN record stop time: {time}", parameter.value);
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
                DateTime IDNLocalHour = lastEndTime.AddHours(-1);
                var recordRequest = new bethistoryRequest
                {
                    date = IDNLocalHour.ToString("yyyy-MM-dd"),
                    from = IDNLocalHour.ToString("HH:mm:ss"),
                    to = IDNLocalHour.AddMinutes(1).AddSeconds(-1).ToString("HH:mm:ss")
                };

                await _systemParameterDbService.PutSystemParameter(parameter);

                //從RG富遊取得的原始資料
                List<Bet_History> gameProviderBetRecords = new List<Bet_History>();
                //要寫入的資料
                List<Bet_History> postBetRecords = new List<Bet_History>();


                await _apiInterfaceService.refreashToken("IDNRecordSchedule", true);

                var betRecord = await _apiInterfaceService.Getbethistory(recordRequest);

                if (betRecord.Message == "Unauthenticated.")
                {
                    await _apiInterfaceService.DelToken();
                    await _apiInterfaceService.refreashToken("IDNRecordSchedule", true);
                }
                // 有錯誤就拋
                if (string.IsNullOrEmpty(betRecord.Message) == false && betRecord.success == false && betRecord.response_code != 404)
                {
                    throw new Exception(betRecord.Message);
                }
                else if (betRecord.data is not null && betRecord.data.bet_history.Count > 0)
                {
                    // 加入遊戲商回應的注單集合
                    gameProviderBetRecords.AddRange(betRecord.data.bet_history ?? new List<Bet_History>());
                }


                if (gameProviderBetRecords.Any() == true)
                {
                    // 排除重複注單
                    postBetRecords = gameProviderBetRecords.DistinctBy(record => new { record.id, record.bet_type, record.date }).ToList();

                    if (postBetRecords.Any() == true)
                    {
                        await _apiInterfaceService.PostIDNRecord(postBetRecords);
                    }
                }
            }
            catch (Exception ex)
            {
                TriggerFailOver(parameter, RANGE_OFFSET);
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run IDN record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        private async void TriggerFailOver(t_system_parameter parameter, TimeSpan offTimeSpan)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            Platform PlatformIDN = Platform.IDN;

            var failoverReq = new PullRecordFailoverWithTimeOffset()
            {
                platform = PlatformIDN,
                repairParameter = parameter.value, // 已經是新的時間
                delay = TimeSpan.FromMinutes(5),
                OffTimeSpan = -offTimeSpan         // 轉換成負值，是要找回起始時間
            };

            await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{PlatformIDN}", failoverReq);
        }
    }
}