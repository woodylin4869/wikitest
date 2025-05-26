using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.RLG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RLG.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.RLG;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.Game.RLG.Response.GetBetRecordResponse;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class RlgRecordSchedule : IInvocable
    {
        private readonly ILogger<RlgRecordSchedule> _logger;
        private readonly IRlgInterfaceService _gameRecordService;
        private readonly ICommonService _commonService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "RlgRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-6);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public RlgRecordSchedule(
            ILogger<RlgRecordSchedule> logger
             , ICommonService commonService
            , GameRecordService gameRecordService
            , ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _commonService = commonService;
            _gameRecordService = gameRecordService._rlgInterfaceService;
            _systemParameterDbService = systemParameterDbService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke RlgRecordSchedule on time : {time}", DateTime.Now);

            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.Add(GAP_TIME);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            // 取得上次結束時間
            var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

            // 檢查有無資料，沒資料的話新增預設值
            if (parameter == null)
            {
                var value = DateTime.Now.ToLocalTime();
                var model = new t_system_parameter()
                {
                    key = SYSTEM_PARAMETERS_KEY,
                    value = value.ToString("yyyy-MM-dd HH:mm:ss"),
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    name = "RLG取得注單排程",
                    description = "RLG記錄end_time"
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
                _logger.LogInformation("{platform} record stop time: {time}", Platform.RLG, parameter.value);
                await Task.CompletedTask;
                return;
            }

            if (Convert.ToDateTime(parameter.value) >= nextTime)
            {
                _logger.LogInformation("return {platform} record schedule current Time : {now} report time : {reportTime} ", Platform.RLG, now, parameter.value);
                return; // 時間不變就結束排程
            }

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = lastEndTime.Add(RANGE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss");
            await _systemParameterDbService.PutSystemParameter(parameter);
            try
            {

                var response = await _gameRecordService.GetRlgBetRecords(lastEndTime, lastEndTime.Add(RANGE_OFFSET).AddSeconds(-1));
                if (!response.Any()) return;

                foreach (var group in response.GroupBy(b => b.createtime.Ticks / TimeSpan.FromHours(3).Ticks))
                {
                    await _gameRecordService.PostRlgRecord(group.ToList());
                }
            }
            catch(Exception ex)
            {
                TriggerFailOver(parameter);
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError(ex, "Run {platform} record schedule Error!", Platform.RLG);
            }
        }

        private async void TriggerFailOver(t_system_parameter parameter)
        {
            try
            {
                if (parameter is null)
                {
                    throw new ArgumentNullException(nameof(parameter));
                }

                var failoverReq = new PullRecordFailoverRequest()
                {
                    platform = Platform.RLG,
                    repairParameter = parameter.value,
                    delay = TimeSpan.FromMinutes(5)
                };

                await _commonService._cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{Platform.RLG}", failoverReq);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{platform} {action} {level}", Platform.RLG, nameof(TriggerFailOver), LogLevel.Error);
            }
        }
    }
}
