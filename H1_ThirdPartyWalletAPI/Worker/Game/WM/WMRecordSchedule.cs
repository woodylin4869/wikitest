using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Reqserver;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Response;
using H1_ThirdPartyWalletAPI.Model.Game.WM.Request;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.OB;
using H1_ThirdPartyWalletAPI.Service.Game.WM;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class WMRecordSchedule : IInvocable
    {
        private readonly ILogger<WMRecordSchedule> _logger;
        private readonly IWMInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "WMRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private readonly TimeSpan MAX_RANGE_OFFSET = TimeSpan.FromMinutes(30);

        public WMRecordSchedule(ILogger<WMRecordSchedule> logger, IWMInterfaceService apiInterfaceService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _dbService = dbService;
            _apiInterfaceService = apiInterfaceService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke WMRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


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
                    name = "WM取得注單排程",
                    description = "WM記錄end_time"
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
                _logger.LogInformation("WM record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            var lastEndTime = DateTime.Parse(parameter.value);

            if (lastEndTime >= nextTime)
            {
                return;
            }

            //nextTime or lastEndTime.Add(MAX_RANGE_OFFSET) 避免拉單範圍超過MAX_RANGE_OFFSET
            parameter.value = new DateTime(Math.Min(nextTime.Ticks, lastEndTime.Add(MAX_RANGE_OFFSET).Ticks)).ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                await _systemParameterDbService.PutSystemParameter(parameter);

                var betLogs = await _apiInterfaceService.GetWMBetRecords(lastEndTime, DateTime.Parse(parameter.value), true);//依結算時間

                if (betLogs.Any())
                    await _apiInterfaceService.PostWMRecord(betLogs);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run WM record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
