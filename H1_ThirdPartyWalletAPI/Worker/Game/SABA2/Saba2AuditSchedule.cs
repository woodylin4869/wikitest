using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Code;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using System.Linq;
using System.Text.Json;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class Saba2AuditSchedule : IInvocable
    {
        private readonly ILogger<Saba2AuditSchedule> _logger;
        private readonly ICommonService _commonService;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly IGameReportDBService _gameReportDBService;

        public Saba2AuditSchedule(ILogger<Saba2AuditSchedule> logger
            , ICommonService commonService
            , IRepairBetRecordService repairBetRecordService
            , ISystemParameterDbService systemParameterDbService
            , IGameReportDBService gameReportDBService)
        {
            _logger = logger;
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
            _logger.LogInformation("Invoke Saba2AuditSchedule on time : {time}", DateTime.Now);
            try
            {
                t_system_parameter parameter = null;

                // 取得當前時間，計算下一個帳務比對的時間
                //var now = DateTime.Now.ToLocalTime().AddDays(-1);
                var now = DateTime.Now.ToLocalTime().AddDays(-1);
                var nextTime = new DateTime(now.Year, now.Month, now.Day);
                var key = "SABA2AuditSchedule";
                // 取得帳務比對的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(key);
                DateTime lastReportTime = new DateTime(now.Year, now.Month, now.Day);
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextTime.ToString("yyyy-MM-dd"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "SABA2 每日時遊戲帳務比對排程",
                        description = "SABA2 紀錄帳務比對排程時間基準點"
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
                        _logger.LogInformation("SABA2 audit schedule stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }


                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        lastReportTime = Convert.ToDateTime(parameter.value).AddDays(1);
                        parameter.value = nextTime.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        _logger.LogInformation("return SABA2 audit schedule current Time : {now} report time : {reportTime} ", now, parameter.value);
                        return; // 時間不變就結束排程
                    }
                }
                await _systemParameterDbService.PutSystemParameter(parameter);
                while (lastReportTime <= Convert.ToDateTime(parameter.value))
                {
                    var compare = await CompareSABA2ndW1Report(Convert.ToDateTime(lastReportTime), Convert.ToDateTime(lastReportTime));
                    //2. 檢查投注次數與輸贏是否一致
                    if (!compare)
                    {
                        //3. 檢查失敗重拉該日遊戲注單
                        RepairBetSummaryReq req = new RepairBetSummaryReq();
                        req.game_id = nameof(Platform.SABA2);
                        req.StartTime = Convert.ToDateTime(lastReportTime).AddHours(12);
                        req.EndTime = req.StartTime;
                        req.SearchType = 2;// 1: 依下注时间查询 2: 依结算日期查询                    
                        await _repairBetRecordService.RepairGameRecord(req);
                    }
                    lastReportTime = Convert.ToDateTime(lastReportTime).AddDays(1);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run saba2 audit schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
        private async Task<bool> CompareSABA2ndW1Report(DateTime startTime, DateTime endTime)
        {
            // 遊戲商的匯總帳
            var tpReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.SABA2,
                ((int)GameReport.e_report_type.FinancalReport).ToString(),
                startTime,
                endTime);

            // 轉帳中心的匯總帳
            var w1ReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.SABA2,
                ((int)GameReport.e_report_type.GameBetRecord).ToString(),
                startTime,
                endTime);

            // 比對遊戲商與轉帳中心的匯總帳
            return tpReportSummary[0].total_count == w1ReportSummary[0].total_count &&
                    tpReportSummary[0].total_bet == w1ReportSummary[0].total_bet &&
                    tpReportSummary[0].total_win == w1ReportSummary[0].total_win &&
                    tpReportSummary[0].total_netwin == w1ReportSummary[0].total_netwin;
        }
    }

}
