using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class JdbAuditSchedule : IInvocable
    {
        private readonly ILogger<JdbAuditSchedule> _logger;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly IGameReportDBService _gameReportDBService;
        public JdbAuditSchedule(ILogger<JdbAuditSchedule> logger
            , IRepairBetRecordService repairBetRecordService
            , IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _repairBetRecordService = repairBetRecordService;
            _gameReportDBService = gameReportDBService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke JdbAuditSchedule on time : {time}", DateTime.Now);
            try
            {
                DateTime dt = DateTime.Parse(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
                IEnumerable<GameReport> result = await _gameReportDBService.GetGameReport(Platform.JDB, null, dt, dt, 0, 10);
                //1. 取得遊戲商財務報表(FinancalReport)與注單每日匯總報表(GameBetRecord)
                var FinancialReport = result.Single(x => x.report_type == (int)GameReport.e_report_type.FinancalReport);
                var RecordDailyReport = result.Single(x => x.report_type == (int)GameReport.e_report_type.GameBetRecord);
                //2. 檢查投注次數與輸贏是否一致
                if (FinancialReport.total_netwin != RecordDailyReport.total_netwin)
                {
                    _logger.LogWarning("Audit JDB report fail Date : {date} FinancalReport : {FinancialReport} RecordDailyReport : {RecordDailyReport}"
                        , dt, JsonSerializer.Serialize(RecordDailyReport).ToString(), JsonSerializer.Serialize(RecordDailyReport).ToString());
                    //3. 檢查失敗重拉該日遊戲注單
                    RepairBetSummaryReq req = new RepairBetSummaryReq();
                    req.game_id = nameof(Platform.JDB);
                    req.StartTime = dt.AddHours(12);
                    req.EndTime = req.StartTime.AddDays(1);
                    req.SearchType = 2;// 1: 依下注时间查询 2: 依结算日期查询                    
                    await _repairBetRecordService.RepairGameRecord(req);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run jdb audit schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }

}
