using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.AspNetCore.Hosting;
using H1_ThirdPartyWalletAPI.Model.Game.JDB;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Exceptions;
using System.Globalization;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class JdbReportSchedule : IInvocable
    {
        private readonly ILogger<JdbReportSchedule> _logger;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        private readonly IJdbDBService _jdbDBService;
        private readonly GameRecordService _gameRecordService;
        private readonly IWebHostEnvironment _env;

        public JdbReportSchedule(ILogger<JdbReportSchedule> logger
            , IGameApiService gameaApiService
            , GameRecordService gameRecordService
            , ICommonService commonService
            , IJdbDBService jdbDBService
            , IWebHostEnvironment env)
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameaApiService;
            _gameRecordService = gameRecordService;
            _env = env;
            _jdbDBService = jdbDBService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke JdbReportSchedule on time : {time}", DateTime.Now);
            try
            {
                GetDailyReportRequest req = new GetDailyReportRequest();
                DateTime dt = DateTime.Now.AddDays(-1);
                req.date = dt.ToString("dd-MM-yyyy");

                //取得老虎機報表
                req.gType = (int)JDB.gType.SLOT;
                await getReport(req);
                //取得魚機報表
                req.Ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                req.gType = (int)JDB.gType.FISH;
                await getReport(req);
                //取得街機報表
                req.Ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                req.gType = (int)JDB.gType.ARCADE;
                await getReport(req);
                //取得BINGO報表
                req.Ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                req.gType = (int)JDB.gType.BINGO;
                await getReport(req);

                DateTime reportDate = DateTime.ParseExact(req.date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                //將所有JDB報表匯總遊戲日報表
                await _gameRecordService._JdbInterfaceService.SummaryGameProviderReport(reportDate, reportDate);
                //將所有JDB注單匯總遊戲日報表
                await _gameRecordService._JdbInterfaceService.SummaryW1Report(reportDate, reportDate);

                await Task.CompletedTask;
            }
            catch (JDBBadRequestException ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run jdb report schedule exception status : {status}  MSG : {Message} ", ex.status, ex.err_text);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run jdb report schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
        private async Task getReport(GetDailyReportRequest req)
        {
            DateTime reportDate = DateTime.ParseExact(req.date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            GetDailyReportRepsonse res = await _gameApiService._JdbAPI.Action42_DailyReport(req);
            if (res.Data.Count > 0)
            {
                foreach (DaliyReportContent jdbReport in res.Data)
                {
                    jdbReport.financialdate = reportDate;
                    jdbReport.gtype = req.gType;
                }
                await _jdbDBService.DeleteJdbReport(res.Data);
                await _jdbDBService.PostJdbReport(res.Data);
            }
        }

    }

}
