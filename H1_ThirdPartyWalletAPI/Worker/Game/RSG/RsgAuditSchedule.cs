using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// 每小時檢查 Rsg 注單資料排程
    /// </summary>
    public class RsgAuditSchedule : IInvocable
    {
        private readonly ILogger<RsgAuditSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly ICommonService _commonService;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        public RsgAuditSchedule(ILogger<RsgAuditSchedule> logger, IGameApiService gameApiService, ICommonService commonService, IRepairBetRecordService repairBetRecordService, ISystemParameterDbService systemParameterDbService, IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _commonService = commonService;
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
            _logger.LogInformation("Invoke RsgAuditSchedule on time : {time}", DateTime.Now);
            try
            {
                t_system_parameter parameter = null;

                // 取得當前時間，計算下一個帳務比對的時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddMinutes(-40);
                var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, (now.Minute / 5) * 5, 0);
                var key = "RsgAuditSchedule2";

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
                        name = "RSG 每小時遊戲帳務比對排程",
                        description = "RSG 紀錄帳務比對排程時間基準點"
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
                    // 紀錄的時間要是五分鐘匯總，不然找不到
                    var currentTime = Convert.ToDateTime(parameter.value);
                    var currentSummaryTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, (currentTime.Minute / 5) * 5, 0);

                    if (currentSummaryTime < nextTime)
                    {
                        parameter.value = currentSummaryTime.AddMinutes(5).ToString("yyyy-MM-dd HH:mm");
                    }
                    else
                    {
                        _logger.LogInformation("return rsg audit schedule current Time : {now} report time : {report time} ", now, parameter.value);
                        return; // 時間不變就結束排程
                    }
                }

                // 排程開關
                if (int.Parse(parameter.min_value) == 0)
                {
                    _logger.LogInformation("rsg audit schedule stop time: {time}", parameter.value);
                    await Task.CompletedTask;
                    return;
                }


                var compare = await CompareRSGAndW1Report(Convert.ToDateTime(parameter.value), Convert.ToDateTime(parameter.value).AddMinutes(5).AddSeconds(-1));

                //若RSG及W1匯總帳有差異則進行補單
                if (!compare)
                {
                    await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                    {
                        game_id = Platform.RSG.ToString(),
                        StartTime = Convert.ToDateTime(parameter.value),
                        EndTime = Convert.ToDateTime(parameter.value).AddMinutes(5).AddSeconds(-1)
                    });
                }

                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run rsg audit schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        /// <summary>
        /// 比對RSG及W1匯總帳是否相同
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<bool> CompareRSGAndW1Report(DateTime startTime, DateTime endTime)
        {
            // 遊戲商的匯總帳
            var rsgReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.RSG,
                ((int)GameReport.e_report_type.FinancalReport).ToString(),
                startTime,
                endTime);

            // 轉帳中心的匯總帳
            var w1ReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.RSG,
                ((int)GameReport.e_report_type.GameBetRecord).ToString(),
                startTime,
                endTime);

            // 比對遊戲商與轉帳中心的匯總帳
            return rsgReportSummary[0].total_count == w1ReportSummary[0].total_count &&
                    rsgReportSummary[0].total_bet == w1ReportSummary[0].total_bet &&
                    rsgReportSummary[0].total_netwin == w1ReportSummary[0].total_netwin;
        }
    }
}
