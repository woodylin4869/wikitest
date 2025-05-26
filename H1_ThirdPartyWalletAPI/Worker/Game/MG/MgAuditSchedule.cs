using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// 每日檢查 MG 注單資料排程
    /// </summary>
    public class MgAuditSchedule : IInvocable
    {
        private readonly ILogger<MgAuditSchedule> _logger;
        private readonly ICommonService _commonService;
        private readonly IDBService _dbService;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly IGameReportDBService _gameReportDBService;

        private const string SYSTEM_PARAMETERS_KEY = "MgAuditSchedule";

        public MgAuditSchedule(ILogger<MgAuditSchedule> logger, ICommonService commonService, IDBService dbService, IRepairBetRecordService repairBetRecordService, ISystemParameterDbService systemParameterDbService, IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _dbService = dbService;
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

            _logger.LogInformation($"Invoke MgAuditSchedule on time : {DateTime.Now.ToLocalTime()}");
            try
            {
                t_system_parameter parameter = null;
                // 取得當前時間，計算下一個帳務比對的時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddHours(-2);
                var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

                // 取得帳務比對的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = SYSTEM_PARAMETERS_KEY,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "MG 每小時遊戲帳務比對排程",
                        description = "MG 紀錄帳務比對排程時間基準點"
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
                        _logger.LogInformation("MG audit schedule stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    var reportTime = Convert.ToDateTime(parameter.value);
                    if (reportTime < nextTime)
                    {
                        var lastReportTime = reportTime.AddHours(1);
                        parameter.value = lastReportTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        _logger.LogInformation("return MG audit schedule current Time : {now} report time : {report time} ", now, parameter.value);
                        return; // 時間不變就結束排程
                    }
                }

                var StartTime = Convert.ToDateTime(parameter.value);
                var EndTime = StartTime.AddHours(1);

                var compare = await CompareMgAndW1Report(StartTime, EndTime);

                //若 MG 及 W1 匯總帳有差異則進行補單
                if (!compare)
                {
                    await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                    {
                        game_id = Platform.MG.ToString(),
                        StartTime = StartTime,
                        EndTime = EndTime
                    });
                }

                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run MG audit schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        /// <summary>
        /// 比對 MG 及 W1 匯總帳是否相同
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<bool> CompareMgAndW1Report(DateTime startTime, DateTime endTime)
        {
            // 遊戲商的匯總帳
            var tpReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.MG,
                ((int)GameReport.e_report_type.FinancalReport).ToString(),
                startTime,
                endTime);

            // 轉帳中心的匯總帳
            var w1ReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.MG,
                ((int)GameReport.e_report_type.GameBetRecord).ToString(),
                startTime,
                endTime);

            // 比對遊戲商與轉帳中心的匯總帳
            return tpReportSummary[0].total_bet == w1ReportSummary[0].total_bet &&
                    tpReportSummary[0].total_win == w1ReportSummary[0].total_win &&
                    tpReportSummary[0].total_netwin == w1ReportSummary[0].total_netwin;
        }
    }
}
