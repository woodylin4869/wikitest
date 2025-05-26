using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.MT;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.MT
{
    public class MTAuditSchedule: IInvocable
    {
        private readonly ILogger<MTAuditSchedule> _logger;
        private readonly IDBService _dbService;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly IGameReportDBService _gameReportDBService;

        private const string SYSTEM_PARAMETERS_KEY = "MTAuditSchedule";

        public MTAuditSchedule(ILogger<MTAuditSchedule> logger, IDBService dbService, IRepairBetRecordService repairBetRecordService, ISystemParameterDbService systemParameterDbService, IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _dbService = dbService;
            _repairBetRecordService = repairBetRecordService;
            _gameReportDBService = gameReportDBService;
            _systemParameterDbService = systemParameterDbService;
        }
        public async Task Invoke()
        {
            _logger.LogInformation("Invoke MTAuditSchedule on time : {time}", DateTime.Now.ToLocalTime());
            try
            {
                t_system_parameter parameter = null;

                // 取得當前時間，計算下一個帳務比對的時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddDays(-1);
                var nextTime = new DateTime(now.Year, now.Month, now.Day);
                DateTime lastReportTime = new DateTime(now.Year, now.Month, now.Day);
                // 取得帳務比對的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = SYSTEM_PARAMETERS_KEY,
                        value = nextTime.ToString("yyyy-MM-dd"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "MT 每小時遊戲帳務比對排程",
                        description = "MT 紀錄帳務比對排程時間基準點"
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
                        _logger.LogInformation("MT audit schedule stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }


                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        lastReportTime = Convert.ToDateTime(parameter.value).AddDays(1);
                        parameter.value = lastReportTime.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        _logger.LogInformation("return MT audit schedule current Time : {now} report time : {reportTime} ", now, parameter.value);
                        return; // 時間不變就結束排程
                    }
                }
                while (lastReportTime <= nextTime)
                {
                    await _systemParameterDbService.PutSystemParameter(parameter);

                    var compare = await CompareMTAndW1Report(Convert.ToDateTime(parameter.value), Convert.ToDateTime(parameter.value));
                    if (!compare)
                    {
                        await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                        {
                            game_id = Platform.MT.ToString(),
                            StartTime = lastReportTime,
                            EndTime = lastReportTime.AddDays(1)
                        });
                    }
                    lastReportTime = lastReportTime.AddDays(1);

                    parameter.value = lastReportTime.ToString("yyyy-MM-dd");
                }
                // 查詢時間寫回 DB
                
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run MT audit schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
        private async Task<bool> CompareMTAndW1Report(DateTime startTime, DateTime endTime)
        {
            // 遊戲商的匯總帳
            var ReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.MT,
                ((int)GameReport.e_report_type.FinancalReport).ToString(),
                startTime,
                endTime);

            // 轉帳中心的匯總帳
            var w1ReportSummary = await _gameReportDBService.GetGameReportSummary(Platform.MT,
                ((int)GameReport.e_report_type.GameBetRecord).ToString(),
                startTime,
                endTime);

            // 比對遊戲商與轉帳中心的匯總帳
            return ReportSummary[0].total_count == w1ReportSummary[0].total_count &&
                    ReportSummary[0].total_bet == w1ReportSummary[0].total_bet &&
                    ReportSummary[0].total_win == w1ReportSummary[0].total_win &&
                    ReportSummary[0].total_netwin == w1ReportSummary[0].total_netwin;
        }
    }
}
