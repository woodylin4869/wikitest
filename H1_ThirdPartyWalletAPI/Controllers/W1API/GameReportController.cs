using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;
using H1_ThirdPartyWalletAPI.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.AspNetCore.Hosting;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System.Linq;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GameReportController : ControllerBase
    {
        private readonly ILogger<GameReportController> _logger;
        private readonly IGameApiService _gameaApiService;
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IGameReportDBService _gameReportDBService; 
        public GameReportController(ILogger<GameReportController> logger
        , ICommonService commonService
        , IGameApiService gameaApiService
        , IWebHostEnvironment env
        , IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _gameaApiService = gameaApiService;
            _commonService = commonService;
            _env = env;
            _gameReportDBService = gameReportDBService;
        }
        /// <summary>
        /// 取得遊戲報表
        /// </summary>
        [HttpGet]
        async public Task<GetGameReport> Get([FromQuery] GetGameReportReq gameReportReq)
        {
            GetGameReport res = new GetGameReport();
            try
            {
                gameReportReq.Platform = (gameReportReq.Platform == null) ? nameof(Platform.ALL) : gameReportReq.Platform;
                Platform platformid = (Platform)Enum.Parse(typeof(Platform), gameReportReq.Platform.ToUpper());
                IEnumerable<GameReport> result = await _gameReportDBService.GetGameReport(platformid, gameReportReq.ReportType, gameReportReq.StartTime, gameReportReq.EndTime, gameReportReq.Page, gameReportReq.Count);
                res.Data = result.ToList();
                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.GetGameListFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameListFail] + " | " + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get gamelist exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 取得遊戲報表總計
        /// </summary>
        [HttpGet("summary")]
        public async Task<GetGameReportSummary> GetSummary([FromQuery] GetGameReportSummaryReq gameReportReq)
        {
            GetGameReportSummary res = new GetGameReportSummary();
            try
            {
                gameReportReq.Platform = (gameReportReq.Platform == null) ? nameof(Platform.ALL) : gameReportReq.Platform;
                Platform platformid = (Platform)Enum.Parse(typeof(Platform), gameReportReq.Platform.ToUpper());
                IEnumerable<dynamic> result = await _gameReportDBService.GetGameReportSummary(platformid, gameReportReq.ReportType, gameReportReq.StartTime, gameReportReq.EndTime);
                res.Count = (int)result.SingleOrDefault().count;
                res.TotalBetCount = result.SingleOrDefault().total_count == null ? 0 : (int)result.SingleOrDefault().total_count;
                res.TotalBet = result.SingleOrDefault().total_bet == null ? 0 : (int)result.SingleOrDefault().total_bet; ;
                res.TotalWin = result.SingleOrDefault().total_win == null ? 0 : (int)result.SingleOrDefault().total_win; ;
                res.TotalNetWin = result.SingleOrDefault().total_netwin == null ? 0 : (int)result.SingleOrDefault().total_netwin; ;
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get gamelist summary exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// 取得指定ID報表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<GetGameReport> Get(long id)
        {
            GetGameReport res = new GetGameReport();
            try
            {
                IEnumerable<GameReport> result = await _gameReportDBService.GetGameReport(id);
                res.Data = result.ToList();
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("get gamelist summary exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }

        /// <summary>
        /// 更新指定ID報表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<ResCodeBase> Put([FromBody] PutGameReportReq req, long id)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                IEnumerable<GameReport> result = await _gameReportDBService.GetGameReport(id);
                var reportdata = result.Single();
                req.TotalCount = (req.TotalCount == null) ? reportdata.total_count : req.TotalCount;
                req.TotalBet = (req.TotalBet == null) ? reportdata.total_bet : req.TotalBet;
                req.TotalWin = (req.TotalWin == null) ? reportdata.total_win : req.TotalWin;
                req.TotalNetwin = (req.TotalNetwin == null) ? reportdata.total_netwin : req.TotalNetwin;
                req.ReportDateTime = (req.ReportDateTime == null) ? reportdata.report_datetime : req.ReportDateTime;
                if (await _gameReportDBService.PutGameReport(id, req) != 1)
                    throw new Exception("update report fail");
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("put gamelist id EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 刪除指定ID報表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<ResCodeBase> Delete(long id)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                if (await _gameReportDBService.DeleteGameReport(id) != 1)
                    throw new Exception("delete game report fail");
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("delete game report exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 新增報表
        /// </summary>
        [HttpPost]
        async public Task<ResCodeBase> Post([FromBody] PostGameReportReq gamereportReq)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                GameReport reportData = new GameReport();
                reportData.platform = gamereportReq.Platform;
                reportData.report_datetime = gamereportReq.ReportDateTime;
                reportData.total_count = gamereportReq.TotalCount;
                reportData.total_bet = gamereportReq.TotalBet;
                reportData.total_win = gamereportReq.TotalWin;
                reportData.report_type = gamereportReq.ReportType;
                if (await _gameReportDBService.PostGameReport(reportData) != 1)
                    throw new Exception("insert game report fail");
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Post game report exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
