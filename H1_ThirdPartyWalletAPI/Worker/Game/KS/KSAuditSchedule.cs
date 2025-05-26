using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.KS;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class KSAuditSchedule : IInvocable
    {
        private readonly ILogger<KSAuditSchedule> _logger;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly ICommonService _commonService;
        private readonly IGameReportDBService _gameReportDBService;
        private const string SYSTEM_PARAMETERS_KEY = "KSAuditSchedule";
        public KSAuditSchedule(ILogger<KSAuditSchedule> logger,
           IRepairBetRecordService repairBetRecordService,
            ICommonService commonService,
           ISystemParameterDbService systemParameterDbService, 
           IGameReportDBService gameReportDBService)
        {
            _logger = logger;
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
            _logger.LogInformation("Invoke KSAuditSchedule on time : {time}", DateTime.Now.ToLocalTime());
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
                        name = "KS 每日時遊戲帳務比對排程",
                        description = "KS 紀錄帳務比對排程時間基準點"
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
                        _logger.LogInformation("KS audit schedule stop time: {time}", parameter.value);
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
                        _logger.LogInformation("return KS audit schedule current Time : {now} report time : {reportTime} ", now, parameter.value);
                        return; // 時間不變就結束排程
                    }
                }

                while (Convert.ToDateTime(lastReportTime) <= Convert.ToDateTime(parameter.value))
                {
                    var compare = await CompareKSAndW1Report(Convert.ToDateTime(lastReportTime), Convert.ToDateTime(lastReportTime));

                    //若KS及W1匯總帳有差異則進行補單
                    if (!compare)
                    {
                        await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                        {
                            game_id = Platform.KS.ToString(),
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
                _logger.LogError("Run KS audit schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        /// <summary>
        /// 比對KS及W1匯總帳是否相同
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<bool> CompareKSAndW1Report(DateTime startTime, DateTime endTime)
        {
            // 遊戲商的匯總帳
            var KSReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.KS,
                ((int)GameReport.e_report_type.FinancalReport).ToString(),
                startTime,
                endTime);

            // 轉帳中心的匯總帳
            var w1ReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.KS,
                ((int)GameReport.e_report_type.GameBetRecord).ToString(),
                startTime,
                endTime);

            // 比對遊戲商與轉帳中心的匯總帳
            return KSReportSummary[0].total_count == w1ReportSummary[0].total_count;

             //&&
             //       KSReportSummary[0].total_bet == w1ReportSummary[0].total_bet &&
             //       KSReportSummary[0].total_win == w1ReportSummary[0].total_win &&
             //       KSReportSummary[0].total_netwin == w1ReportSummary[0].total_netwin
        }
    }
}
