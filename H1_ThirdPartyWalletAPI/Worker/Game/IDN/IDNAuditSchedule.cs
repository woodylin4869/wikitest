using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class IDNAuditSchedule : IInvocable
    {
        private readonly ILogger<IDNAuditSchedule> _logger;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly IGameReportDBService _gameReportDBService;
        private const string SYSTEM_PARAMETERS_KEY = "IDNAuditSchedule";
        public IDNAuditSchedule(ILogger<IDNAuditSchedule> logger,
           IRepairBetRecordService repairBetRecordService,
           ISystemParameterDbService systemParameterDbService,
           IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _repairBetRecordService = repairBetRecordService;
            _systemParameterDbService = systemParameterDbService;
            _gameReportDBService = gameReportDBService;
        }

        /// <summary>
        /// 流程大綱
        /// 1. 比對轉帳中心與遊戲商的匯總帳是否一致
        /// 2. 如果帳務不一致的話，啟動補單機制
        /// 3. 將最後匯總結果寫回 DB
        /// </summary>
        /// <returns></returns>
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke IDNAuditSchedule on time : {time}", DateTime.Now.ToLocalTime());
            try
            {
                t_system_parameter parameter = null;

                // 取得當前時間，計算下一個帳務比對的時間
                //var now = DateTime.Now.ToLocalTime().AddDays(-1);
                var now = DateTime.Now.ToLocalTime();
                var nextTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

                // 取得帳務比對的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);
                string lastReportTime = string.Empty;
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    lastReportTime = DateTime.Now.ToLocalTime().AddDays(-1).ToString("yyyy-MM-dd");
                    var model = new t_system_parameter()
                    {
                        key = SYSTEM_PARAMETERS_KEY,
                        value = nextTime.ToString("yyyy-MM-dd 00:00:00"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "IDN 每日遊戲帳務比對排程",
                        description = "IDN 紀錄帳務比對排程時間基準點"
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
                        _logger.LogInformation("IDN audit schedule stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }


                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        lastReportTime = Convert.ToDateTime(parameter.value).AddDays(1).ToString("yyyy-MM-dd");
                        parameter.value = nextTime.AddDays(-1).ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        _logger.LogInformation("return IDN audit schedule current Time : {now} report time : {reportTime} ", now, parameter.value);
                        return; // 時間不變就結束排程
                    }
                }

                while (Convert.ToDateTime(lastReportTime) <= Convert.ToDateTime(parameter.value))
                {
                    var compare = await CompareIDNAndW1Report(Convert.ToDateTime(lastReportTime), Convert.ToDateTime(lastReportTime));

                    //若IDN及W1匯總帳有差異則進行補單
                    if (!compare)
                    {
                        await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                        {
                            game_id = Platform.IDN.ToString(),
                            StartTime = Convert.ToDateTime(lastReportTime),
                            EndTime = Convert.ToDateTime(lastReportTime).AddDays(1)
                        });
                    }
                    lastReportTime = Convert.ToDateTime(lastReportTime).AddDays(1).ToString("yyyy-MM-dd");
                }

                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run IDN audit schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        /// <summary>
        /// 比對IDN及W1匯總帳是否相同
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<bool> CompareIDNAndW1Report(DateTime startTime, DateTime endTime)
        {
            // 遊戲商的匯總帳
            var IDNReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.IDN,
                ((int)GameReport.e_report_type.FinancalReport).ToString(),
                startTime,
                endTime);

            // 轉帳中心的匯總帳
            var w1ReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.IDN,
                ((int)GameReport.e_report_type.GameBetRecord).ToString(),
                startTime,
                endTime);

            // 比對遊戲商與轉帳中心的匯總帳
            return IDNReportSummary[0].total_bet == w1ReportSummary[0].total_bet &&
                   IDNReportSummary[0].total_win == w1ReportSummary[0].total_win &&
                   IDNReportSummary[0].total_netwin == w1ReportSummary[0].total_netwin;
        }

    }
}
