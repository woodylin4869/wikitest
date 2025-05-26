using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using H1_ThirdPartyWalletAPI.Code;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Enum;

namespace H1_ThirdPartyWalletAPI.Worker.Game.RGRICH
{
    public class RGRICHAuditSchedule : IInvocable
    {
        private readonly ILogger<RGRICHAuditSchedule> _logger;
        private readonly IDBService _dbService;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly IGameReportDBService _gameReportDBService;

        private const string SYSTEM_PARAMETERS_KEY = "RGRICHAuditSchedule";

        public RGRICHAuditSchedule(ILogger<RGRICHAuditSchedule> logger, IDBService dbService, IRepairBetRecordService repairBetRecordService, ISystemParameterDbService systemParameterDbService, IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _dbService = dbService;
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

            _logger.LogInformation("Invoke RGRICHAuditSchedule on time : {time}", DateTime.Now.ToLocalTime());
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
                        name = "RGRICH 每小時遊戲帳務比對排程",
                        description = "RGRICH 紀錄帳務比對排程時間基準點"
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
                        _logger.LogInformation("RGRICH audit schedule stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }

                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        lastReportTime = Convert.ToDateTime(parameter.value).AddHours(1);
                        parameter.value = nextTime.ToString("yyyy-MM-dd HH:00:00");
                    }
                    else
                    {
                        _logger.LogInformation("return RGRICH audit schedule current Time : {now} report time : {reportTime} ", now, parameter.value);
                        return; // 時間不變就結束排程
                    }
                }

                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);
                DateTime startTime = lastReportTime;

                while (true)
                {
                    DateTime endTime = startTime.AddHours(1);
                    if (startTime > nextTime)
                    {
                        break;
                    }
                    _logger.LogInformation("Invoke RGRICHAuditSchedule on time : {time}", startTime);

                    var compare = await CompareRGRICHAndW1Report(startTime,
                        endTime);

                    if (!compare)
                    {
                        // 自動補單預設查詢遊戲商注單模式為下注時間
                        await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                        {
                            SearchType = (int)SearchMode.BetTime,
                            game_id = Platform.RGRICH.ToString(),
                            StartTime = startTime,
                            EndTime = endTime
                        });
                    }

                    startTime = endTime;
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run RGRICH audit schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        /// <summary>
        /// 比對TP及W1匯總帳是否相同
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<bool> CompareRGRICHAndW1Report(DateTime startTime, DateTime endTime)
        {
            // 遊戲商的匯總帳
            var ReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.RGRICH,
                ((int)GameReport.e_report_type.FinancalReport).ToString(),
                startTime,
                endTime);

            // 轉帳中心的匯總帳
            var w1ReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.RGRICH,
                ((int)GameReport.e_report_type.GameBetRecord).ToString(),
                startTime,
                endTime);

            // 比對遊戲商與轉帳中心的匯總帳
            return
                    ReportSummary[0].total_bet == w1ReportSummary[0].total_bet &&
                    ReportSummary[0].total_win == w1ReportSummary[0].total_win &&
                    ReportSummary[0].total_netwin == w1ReportSummary[0].total_netwin;
        }
    }
}