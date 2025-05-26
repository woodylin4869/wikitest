using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.PP
{
    public class PPReportSchedulecs: IInvocable
    {
        private readonly ILogger<PPReportSchedulecs> _logger;
        private readonly GameRecordService _gameRecordService;
        private readonly ICommonService _commonService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        public PPReportSchedulecs(ILogger<PPReportSchedulecs> logger,
            GameRecordService gameRecordService,
            ICommonService commonService,
            ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameRecordService = gameRecordService;
            _commonService = commonService;
            _systemParameterDbService = systemParameterDbService;
        }


        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke PPReportSchedule on time : {time}", DateTime.Now);
            t_system_parameter parameter = null;

            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.AddHours(-2);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

            try
            {
                var key = "PPReportSchedule";

                // 取得同步 PP 每小時遊戲匯總報表的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(key);
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "PP 每小時遊戲匯總報表排程",
                        description = "PP 紀錄排程時間基準點"
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
                        _logger.LogInformation("PP report stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        var lastReportTime = Convert.ToDateTime(parameter.value);
                        parameter.value = lastReportTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        return; // 時間不變就結束排程
                    }
                }

                // 產生W1 PP 每小時報表
                await _gameRecordService._PPInterfaceService.SummaryW1Report(DateTime.Parse(parameter.value), DateTime.Parse(parameter.value).AddHours(1));
                // 產生Game PP 每小時報表
                await _gameRecordService._PPInterfaceService.SummaryGameProviderReport(DateTime.Parse(parameter.value), DateTime.Parse(parameter.value).AddHours(1));
                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);

            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run PP report schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
