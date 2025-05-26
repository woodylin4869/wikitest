using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common.DB.ClickHouse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API

{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GetPlayGameReportController : ControllerBase
    {
        private readonly ILogger<GetPlayGameReportController> _logger;
        private readonly IBetSummaryReportDBService _betSummaryReportDBService;
        public GetPlayGameReportController(ILogger<GetPlayGameReportController> logger
            , IBetSummaryReportDBService betSummaryReportDBService)
        {
            _logger = logger;
            _betSummaryReportDBService = betSummaryReportDBService;
        }

        /// <summary>
        /// 使用遊戲trans_id查詢詳細遊戲結果
        /// </summary>
        [HttpGet]
        async public Task<GetPlayGameReport> Get([FromQuery] GetPlayGameReportReq PlayGameReportReq)
        {
            GetPlayGameReport res = new GetPlayGameReport();
            try
            {
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];

                var RecordDetailRes = await _betSummaryReportDBService.GetPlayerSummaryDay(PlayGameReportReq.Club_id, PlayGameReportReq.ReportTime);

                res.Data = RecordDetailRes.OrderByDescending(e => e.Platform).ThenBy(e => e.GameId).Select(obj => new PlayGameReport
                {
                    Club_id = obj.ClubId,
                    platform = obj.Platform,
                    report_time = obj.ReportDate.ToString("yyyy-MM-dd"),
                    TotalBetCount = obj.TotalCount,
                    game_id = obj.GameId,
                    TotalBet = obj.BetAmount,
                    TotalWin = obj.WinAmount,
                    TotalNetWin = obj.NetWinAmount,
                    JackPot = obj.JackPot
                }).ToList();
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get GetPlayGameReport EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetGameRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameRecordFail] + " | " + ex.Message.ToString();
                return res;
            }
        }
    }
}
