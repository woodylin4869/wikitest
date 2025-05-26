using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// 每日檢查 META 注單資料排程
    /// </summary>
    public class MetaAuditSchedule : IInvocable
    {
        private readonly ILogger<MetaAuditSchedule> _logger;
        private readonly ICommonService _commonService;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        public MetaAuditSchedule(ILogger<MetaAuditSchedule> logger, ICommonService commonService, IRepairBetRecordService repairBetRecordService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _commonService = commonService;
            _repairBetRecordService = repairBetRecordService;
            _systemParameterDbService = systemParameterDbService;
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
            _logger.LogInformation("Invoke MetaAuditSchedule on time : {time}", DateTime.Now);
            try
            {
                t_system_parameter parameter = null;
                // 取得當前時間，計算下一個帳務比對的時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddDays(-1);
                var nextTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                var key = "MetaAuditSchedule";

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
                        name = "META 每日檢查遊戲帳務比對排程",
                        description = "META 紀錄帳務比對排程時間基準點"
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
                        _logger.LogInformation("meta audit schedule stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        var lastReportTime = Convert.ToDateTime(parameter.value);
                        parameter.value = lastReportTime.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        _logger.LogInformation("return meta audit schedule current Time : {now} report time : {report time} ", now, parameter.value);
                        return; // 時間不變就結束排程
                    }
                }


                var StartTime = Convert.ToDateTime(parameter.value);
                var EndTime = Convert.ToDateTime(parameter.value).AddDays(1);
                // 遊戲商的匯總帳
                //var metaReportSummary = await _commonService._serviceDB.GetGameReportSummary(Platform.META,
                //    ((int)GameReport.e_report_type.FinancalReport).ToString(),
                //    StartTime,
                //    EndTime);
                // 轉帳中心的匯總帳
                //var w1ReportSummary = await _commonService._serviceDB.GetGameReportSummary(Platform.META,
                //    ((int)GameReport.e_report_type.GameBetRecord).ToString(),
                //    StartTime,
                //    EndTime);


                await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                {
                    game_id = Platform.META.ToString(),
                    StartTime = StartTime,
                    EndTime = EndTime
                });

                // 比對遊戲商與轉帳中心的匯總帳
                //if (metaReportSummary[0].total_netwin != w1ReportSummary[0].total_netwin)
                //{

                //}
                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run meta audit schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
