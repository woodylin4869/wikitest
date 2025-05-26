using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.FC;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class FcReportSchedule : IInvocable
    {
        private readonly ILogger<FcReportSchedule> _logger;
        private readonly IFCInterfaceService _apiInterfaceService;
        private readonly ICommonService _commonService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        public FcReportSchedule(ILogger<FcReportSchedule> logger,
           IFCInterfaceService apiInterfaceService,
            ICommonService commonService,
           ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _commonService = commonService;
            _apiInterfaceService = apiInterfaceService;
            _systemParameterDbService = systemParameterDbService;
        }


        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke FcReportSchedule on time : {time}", DateTime.Now);
            t_system_parameter parameter = null;

            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime().AddDays(-1);

            var nextTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

            try
            {
                var key = "FcReportSchedule";

                // 取得同步 FC 每小時遊戲匯總報表的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(key);
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextTime.ToString("yyyy-MM-dd"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "FC 每日遊戲匯總報表排程",
                        description = "FC 紀錄排程時間基準點"
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
                        _logger.LogInformation("FC report stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        parameter.value = nextTime.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        return; // 時間不變就結束排程
                    }
                }

                // 產生W1 FC 每日報表
                await _apiInterfaceService.SummaryW1Report(DateTime.Parse(parameter.value), DateTime.Parse(parameter.value));
                // 產生Game FC 每日報表
                await _apiInterfaceService.SummaryGameProviderReport(DateTime.Parse(parameter.value), DateTime.Parse(parameter.value));
                //查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);

            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run FC report schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
