using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.MP;
using H1_ThirdPartyWalletAPI.Worker.Game.MP;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using H1_ThirdPartyWalletAPI.Service.Game.RGRICH;

namespace H1_ThirdPartyWalletAPI.Worker.Game.RGRICH
{
    public class RGRICHReportSchedule : IInvocable
    {
        private readonly ILogger<RGRICHReportSchedule> _logger;
        private readonly IRGRICHInterfaceService _apiInterfaceService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        public const string SYSTEM_PARAMETERS_KEY = "RGRICHReportSchedule";
        public RGRICHReportSchedule(ILogger<RGRICHReportSchedule> logger,
            IRGRICHInterfaceService apiInterfaceService,
            ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
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
            _logger.LogInformation("Invoke RGRICHReportSchedule on time : {time}", DateTime.Now);
            t_system_parameter parameter = null;

            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            // 預設2小時後執行
            now = now.AddHours(-2);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
            var lastReportTime = DateTime.Now;
            try
            {
            

                // 取得同步 RGRICH 每小時遊戲匯總報表的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = SYSTEM_PARAMETERS_KEY,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "RGRICH 每小時遊戲匯總報表排程",
                        description = "RGRICH 紀錄排程時間基準點"
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
                        _logger.LogInformation("RGRICH report stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }

                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        lastReportTime = Convert.ToDateTime(parameter.value).AddHours(1);
                        parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        return; // 時間不變就結束排程
                    }
                }
                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);
                // 產生W1  每小時報表
                await _apiInterfaceService.SummaryW1Report(lastReportTime, nextTime);
                // 產生Game 每小時報表
                await _apiInterfaceService.SummaryGameProviderReport(lastReportTime, nextTime);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run RGRICH report schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}