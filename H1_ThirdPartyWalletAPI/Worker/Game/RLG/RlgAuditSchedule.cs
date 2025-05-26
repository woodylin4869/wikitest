using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class RlgAuditSchedule : IInvocable
    {
        private readonly ILogger<RlgAuditSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly ICommonService _commonService;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly IGameReportDBService _gameReportDBService;

        public RlgAuditSchedule(ILogger<RlgAuditSchedule> logger, IGameApiService gameApiService, ICommonService commonService, IRepairBetRecordService repairBetRecordService, ISystemParameterDbService systemParameterDbService, IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _commonService = commonService;
            _repairBetRecordService = repairBetRecordService;
            _systemParameterDbService = systemParameterDbService;
            _gameReportDBService = gameReportDBService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke RlgAuditSchedule on time : {time}", DateTime.Now);
            t_system_parameter parameter = null;

            // 取得當前時間，計算下一個帳務比對的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.AddHours(-2);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

            try
            {
                var key = "RlgAuditSchedule";

                // 取得帳務比對的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(key);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        min_value = string.Format("{0}", 1),
                        name = "RLG 每小時遊戲帳務比對排程",
                        description = "RLG 紀錄帳務比對排程時間基準點"
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
                        _logger.LogInformation("Rlg audit stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        var lastReportTime = Convert.ToDateTime(parameter.value);
                        parameter.value = lastReportTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        return; // 時間不變就結束排程
                    }
                }
                // 遊戲商的匯總帳
                var rlgReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.RLG,
                    ((int)GameReport.e_report_type.FinancalReport).ToString(),
                   Convert.ToDateTime(parameter.value),
                    Convert.ToDateTime(parameter.value).AddHours(1).AddSeconds(-1));

                // 轉帳中心的匯總帳
                var w1ReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.RLG,
                    ((int)GameReport.e_report_type.GameBetRecord).ToString(),
                    Convert.ToDateTime(parameter.value),
                    Convert.ToDateTime(parameter.value).AddHours(1).AddSeconds(-1));

                // 比對遊戲商與轉帳中心的匯總帳
                if (rlgReportSummary[0].total_count == w1ReportSummary[0].total_count &&
                    rlgReportSummary[0].total_bet == w1ReportSummary[0].total_bet &&
                    rlgReportSummary[0].total_win == w1ReportSummary[0].total_win &&
                    rlgReportSummary[0].total_netwin == w1ReportSummary[0].total_netwin)
                {
                    //return;
                }


                await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                {
                    game_id = Platform.RLG.ToString(),
                    StartTime = Convert.ToDateTime(parameter.value),
                    EndTime = Convert.ToDateTime(parameter.value).AddHours(1).AddSeconds(-1)
                });
                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run rlg audit schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
