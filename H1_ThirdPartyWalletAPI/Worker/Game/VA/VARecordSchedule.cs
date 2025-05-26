using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.VA;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.GameAPI.Service.Game.VA;
using ThirdPartyWallet.Share.Model.Game.VA;
using ThirdPartyWallet.Share.Model.Game.VA.Request;
using ThirdPartyWallet.Share.Model.Game.VA.Response;

namespace H1_ThirdPartyWalletAPI.Worker.Game.VA
{
    public class VARecordSchedule : IInvocable
    {
        private readonly ILogger<VARecordSchedule> _logger;
        private readonly IVAApiService _VAApiService;
        private readonly IVAInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly IOptions<VAConfig> _options;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly ICacheDataService _cacheDataService;

        private const string SYSTEM_PARAMETERS_KEY = "VARecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public VARecordSchedule(ILogger<VARecordSchedule> logger, IVAInterfaceService apiInterfaceService, IVAApiService VAApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService, IOptions<VAConfig> options, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _VAApiService = VAApiService;
            _dbService = dbService;
            _apiInterfaceService = apiInterfaceService;
            _systemParameterDbService = systemParameterDbService;
            _options = options;
            _cacheDataService = cacheDataService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke VARecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


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
                    name = "VA取得注單排程",
                    description = "VA記錄end_time"
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
                _logger.LogInformation("VA record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }
            //parameter.value = "2024-08-16 11:43:00";

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");
            RANGE_OFFSET = nextTime - lastEndTime;
            if (lastEndTime >= nextTime)
            {
                return;
            }
            try
            {
                //要寫入的資料
                List<Betlog> postBetRecords = new List<Betlog>();

                BetlogListByTimeRequest req = new BetlogListByTimeRequest()
                {
                    StartTime = lastEndTime,
                    EndTime = nextTime,
                };

                //單次搜尋的起迄時間區間最大為15分鐘。
                if ((nextTime - lastEndTime).TotalMinutes > 15)
                {
                    req.EndTime = lastEndTime.AddMinutes(15);
                    parameter.value = lastEndTime.AddMinutes(15).ToString("yyyy-MM-dd HH:mm:ss");
                    RANGE_OFFSET = lastEndTime.AddMinutes(15) - lastEndTime;
                }
                await _systemParameterDbService.PutSystemParameter(parameter);


                List<Betlog> gameProviderBetRecords =  await _apiInterfaceService.GetGameBetlogFunc(req.StartTime, req.EndTime);

                if (gameProviderBetRecords.Any() == true)
                {
                    // 排除重複注單
                    postBetRecords = gameProviderBetRecords.DistinctBy(record => new { record.BetId, record.CreateTime }).ToList();

                    if (postBetRecords.Any() == true)
                    {
                        await _apiInterfaceService.PostVARecord(postBetRecords);
                    }
                }
            }
            catch (Exception ex)
            {
                TriggerFailOver(parameter, RANGE_OFFSET);
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run VA record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }


        private async void TriggerFailOver(t_system_parameter parameter, TimeSpan offTimeSpan)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            Platform PlatformVA = Platform.VA;

            var failoverReq = new PullRecordFailoverWithTimeOffset()
            {
                platform = PlatformVA,
                repairParameter = parameter.value, // 已經是新的時間
                delay = TimeSpan.FromMinutes(5),
                OffTimeSpan = -offTimeSpan         // 轉換成負值，是要找回起始時間
            };

            await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{PlatformVA}", failoverReq);
        }
    }
}
