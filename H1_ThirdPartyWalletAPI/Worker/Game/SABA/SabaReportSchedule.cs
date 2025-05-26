using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.AspNetCore.Hosting;
using H1_ThirdPartyWalletAPI.Model.Game;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class SabaReportSchedule : IInvocable
    {
        private readonly ILogger<SabaReportSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly ICommonService _commonService;
        private readonly GameRecordService _gameRecordService;
        private readonly ISabaDbService _sabaDbService;

        public SabaReportSchedule(ILogger<SabaReportSchedule> logger
            , IGameApiService gameaApiService
            , GameRecordService gameRecordService
            , ICommonService commonService
            , ISabaDbService sabaDbService)
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _commonService = commonService;
            _gameRecordService = gameRecordService;
            _sabaDbService = sabaDbService;
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
                _logger.LogInformation("Invoke SabaReportSchedule on time : {time}", DateTime.Now);
                SABA_GetFinancialReport req = new SABA_GetFinancialReport();
                DateTime dt = DateTime.Now.Date.AddDays(-1);
                req.financial_date = dt.ToString("yyyy-MM-dd");
                req.currency = SABA.Currency["THB"];
                //1.從SABA API取得財務報表
                var GetFinacialResponse = await _gameApiService._SabaAPI.GetFinancialReport(req);
                //2.刪除寫入SABA財務報表
                if (GetFinacialResponse.Data != null)
                {
                    await _sabaDbService.DeleteSabaReport(GetFinacialResponse.Data);
                    await _sabaDbService.PostSabaReport(GetFinacialResponse.Data);
                    //3.轉型並寫入遊戲報表
                    await _gameRecordService._SabaInterfaceService.PostGameReport(GetFinacialResponse.Data);
                }
                //5.從SABA Bet Record產出遊戲報表
                await _gameRecordService._SabaInterfaceService.CreateGameReportFromBetRecord(dt);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run saba report schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }

}
