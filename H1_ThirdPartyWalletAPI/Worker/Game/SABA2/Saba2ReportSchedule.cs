using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class Saba2ReportSchedule : IInvocable
    {
        private readonly ILogger<Saba2ReportSchedule> _logger;
        private readonly ISaba2InterfaceService _saba2InterfaceService;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameaApiService;
        private readonly IWebHostEnvironment _env;
        private readonly ISystemParameterDbService _systemParameterDbService;

        public Saba2ReportSchedule(ILogger<Saba2ReportSchedule> logger, ISaba2InterfaceService saba2InterfaceService, ICommonService commonService, IGameApiService gameaApiService, IWebHostEnvironment env, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _saba2InterfaceService = saba2InterfaceService;
            _commonService = commonService;
            _gameaApiService = gameaApiService;
            _env = env;
            _systemParameterDbService = systemParameterDbService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            try
            {
                t_system_parameter parameter = null;
                // 取得當前時間，計算下一個匯總的時間
                var now = DateTime.Now.ToLocalTime().AddDays(-1);

                var lastReportTime= new DateTime(now.Year, now.Month, now.Day);
                var nextTime = new DateTime(now.Year, now.Month, now.Day);

                var key = "SABA2ReportSchedule";

                // 取得同步 SABA2 每小時遊戲匯總報表的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(key);
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextTime.ToString("yyyy-MM-dd"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "SABA2 每天遊戲匯總報表排程",
                        description = "SABA2 紀錄排程時間基準點"
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
                        _logger.LogInformation("SABA2 report stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    if (Convert.ToDateTime(parameter.value) <= nextTime)
                    {
                        lastReportTime = Convert.ToDateTime(parameter.value).AddDays(1);
                        parameter.value = nextTime.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        return; // 時間不變就結束排程
                    }
                }
                await _systemParameterDbService.PutSystemParameter(parameter);
              
                while (true)
                {
                    if (lastReportTime > nextTime)
                    {
                        break;
                    }
                    await _saba2InterfaceService.CreateGameReportFromBetRecord(lastReportTime);
                    SABA_GetFinancialReport req = new SABA_GetFinancialReport();
                    req.financial_date = lastReportTime.ToString("yyyy-MM-dd");

                    var report = new SABA_GetFinancialReportData();
                    foreach(var currency in SABA.Currency.Where(pair => _env.EnvironmentName == "PRD" || pair.Key == "UUS").Select(p => p.Value)) //測試環境只有UUS
                    {
                        req.currency = currency;
                        //1.從SABA API取得財務報表
                        var GetFinacialResponse = await _gameaApiService._Saba2API.GetFinancialReport(req);
                        report.FinancialDate = GetFinacialResponse.Data.FinancialDate;
                        report.TotalBetAmount += GetFinacialResponse.Data.TotalBetAmount;
                        report.TotalWinAmount += GetFinacialResponse.Data.TotalWinAmount;
                    }
                    await _saba2InterfaceService.PostGameReport(report);
                    lastReportTime = lastReportTime.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run saba2 report schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }

    }
}
