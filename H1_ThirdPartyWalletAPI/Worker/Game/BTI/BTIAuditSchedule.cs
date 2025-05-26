using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Game.BTI;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker.Game.BTI
{
    public class BTIAuditSchedule : IInvocable
    {
        private readonly ILogger<BTIAuditSchedule> _logger;
        private readonly IDBService _dbService;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService; 
        private const string SYSTEM_PARAMETERS_KEY = "BTIAuditSchedule";

        public BTIAuditSchedule(
            ILogger<BTIAuditSchedule> logger,
            IDBService dbService,
            IRepairBetRecordService repairBetRecordService,
            ISystemParameterDbService systemParameterDbService
        )
        {
            _logger = logger;
            _dbService = dbService;
            _repairBetRecordService = repairBetRecordService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke {schedule} on time : {time}", SYSTEM_PARAMETERS_KEY, DateTime.Now.ToLocalTime());
            
            try
            {
                t_system_parameter parameter = null;

                // 取得當前時間，計算下一個帳務比對的時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddHours(-2);
                var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
                DateTime lastReportTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
                // 取得帳務比對的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = SYSTEM_PARAMETERS_KEY,
                        value = nextTime.ToString("yyyy-MM-dd HH:00:00"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "BTI每小時遊戲帳務比對排程",
                        description = "BTI紀錄帳務比對排程時間基準點"
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
                else
                {

                    if (int.Parse(parameter.min_value) == 0)
                    {
                        _logger.LogInformation("{schedule} stop time: {time}", SYSTEM_PARAMETERS_KEY, parameter.value);
                        await Task.CompletedTask;
                        return;
                    }


                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        lastReportTime = Convert.ToDateTime(parameter.value).AddHours(1);
                        parameter.value = lastReportTime.ToString("yyyy-MM-dd HH:00:00");
                    }
                    else
                    {

                        _logger.LogInformation("return {schedule} current Time : {now} report time : {reportTime} ", SYSTEM_PARAMETERS_KEY, now, parameter.value);
                        return; // 時間不變就結束排程
                    }
                }

                // BTI 無提供匯總查詢方法 無比對 直接定時都叫補單
                await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                {
                    game_id = Platform.BTI.ToString(),
                    StartTime = lastReportTime.AddHours(-2),
                    EndTime = lastReportTime.AddHours(1).AddMilliseconds(-1)
                });

                // 更新參數
                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("{schedule} exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", SYSTEM_PARAMETERS_KEY, ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
