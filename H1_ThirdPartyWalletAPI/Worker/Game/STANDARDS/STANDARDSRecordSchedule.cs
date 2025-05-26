using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.STANDARDS;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.GameAPI.Service.Game.STANDARDS;
using ThirdPartyWallet.Share.Model.Game.STANDARDS.Request;
using ThirdPartyWallet.Share.Model.Game.STANDARDS.Response;

namespace H1_ThirdPartyWalletAPI.Worker.Game.STANDARDS
{
    public class STANDARDSRecordSchedule : IInvocable
    {
        private readonly ILogger<STANDARDSRecordSchedule> _logger;
        private readonly ISTANDARDSApiService _STANDARDSApiService;
        private readonly ISTANDARDS_InterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly ICacheDataService _cacheDataService;

        private const string SYSTEM_PARAMETERS_KEY = "STANDARDSRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public STANDARDSRecordSchedule(ILogger<STANDARDSRecordSchedule> logger, ISTANDARDS_InterfaceService apiInterfaceService, ISTANDARDSApiService STANDARDSApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _STANDARDSApiService = STANDARDSApiService;
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
            _logger.LogInformation("Invoke STANDARDSRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


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
                    name = "STANDARDS取得注單排程",
                    description = "STANDARDS記錄end_time"
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
                _logger.LogInformation("STANDARDS record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");

            if (lastEndTime >= nextTime)
            {
                return;
            }
            try
            {
                BetlogRequest req = new BetlogRequest()
                {
                    start_time = lastEndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end_time = lastEndTime.AddMinutes(1).AddMilliseconds(-1).ToString("yyyy-MM-ddTHH:mm:ss"),
                    page = 1,
                    page_size = 2000
                };
                parameter.value = lastEndTime.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");
                await _systemParameterDbService.PutSystemParameter(parameter);

                var res =await  _apiInterfaceService.Getbetlog(req);

                if (!res.Any())
                {
                    return;
                }
                await _apiInterfaceService.PostSTANDARDSRecord(res);
            }
            catch (Exception ex)
            {
                TriggerFailOver(parameter, RANGE_OFFSET);
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run STANDARDS record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
        private async void TriggerFailOver(t_system_parameter parameter, TimeSpan offTimeSpan)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            Platform PlatformSTANDARDS = Platform.STANDARDS;

            var failoverReq = new PullRecordFailoverWithTimeOffset()
            {
                platform = PlatformSTANDARDS,
                repairParameter = parameter.value, // 已經是新的時間
                delay = TimeSpan.FromMinutes(5),
                OffTimeSpan = -offTimeSpan         // 轉換成負值，是要找回起始時間
            };

            await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{PlatformSTANDARDS}", failoverReq);
        }
    }
}
