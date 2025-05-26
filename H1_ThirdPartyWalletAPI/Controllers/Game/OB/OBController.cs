

using H1_ThirdPartyWalletAPI.Model.Game.OB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Response;
using H1_ThirdPartyWalletAPI.Service.Game.OB;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.OB
{
    [Route("/[controller]")]
    [ApiController]
    public class OBController : ControllerBase
    {

        private readonly IOBApiService _iobapiservice;
        public OBController(IOBApiService iobapiservice)
        {
            _iobapiservice = iobapiservice;
        }
        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateMember")]
        public async Task<CreateMemberResponse> CreateMemberAsync([FromBody] CreateMemberRequest source)
        {
            var reqserver = new Model.Game.OB.Reqserver.CreateMemberRequest
            {
                loginName = source.LoginName,
                loginPassword = source.LoginPassword,
                lang = Model.Game.OB.OB.lang[source.Lang],
                oddType = source.oddType,
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString().PadRight(13, '0')

            };
            return await _iobapiservice.CreateMemberAsync(reqserver);
        }
        /// <summary>
        /// 取得遊戲URL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GameUrl")]
        public async Task<FastGameResponse> FastGameAsync([FromBody] FastGameRequest source)
        {
            var reqserver = new Model.Game.OB.Reqserver.FastGameReqserver
            {
                loginName = source.loginName,
                loginPassword = source.loginPassword,
                deviceType = source.deviceType,
                oddType = source.oddType,
                backurl = source.backurl,
                lang = Model.Game.OB.OB.lang[source.Lang],
                gameTypeId = source.gameTypeId == -1 ? "" : source.gameTypeId.ToString(),
                showExit = source.showExit,
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString().PadRight(13, '0')
            };
            return await _iobapiservice.FastGameAsync(reqserver);
        }

        /// <summary>
        /// 取得會員額度
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Getbalance")]
        public async Task<GetbalanceResponse> GetbalanceAsync([FromBody] GetbalanceRequest source)
        {
            var reqserver = new Model.Game.OB.Reqserver.GetbalanceReqserver
            {
                loginName=source.loginName,
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString().PadRight(13, '0')
            };
            return await _iobapiservice.GetbalanceAsync(reqserver);
        }
        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("deposit")]
        public async Task<DepositResponse> DepositAsync([FromBody] depositRequest source)
        {
            var reqserver = new Model.Game.OB.Reqserver.depositReqserver
            {
                loginName = source.loginName,
                transferNo=source.transferNo,
                amount = source.amount,
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString().PadRight(13, '0')
            };
            return await _iobapiservice.DepositAsync(reqserver);
        }
        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("withdraw")]
        public async Task<WithdrawResponse> WithdrawAsync([FromBody] WithdrawRequest source)
        {
            var reqserver = new Model.Game.OB.Reqserver.WithdrawReqserver
            {
                loginName = source.loginName,
                transferNo = source.transferNo,
                amount = source.amount,
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString().PadRight(13, '0')
            };
            return await _iobapiservice.WithdrawAsync(reqserver);
        }
        /// <summary>
        /// 查詢交易紀錄狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("transfer")]
        public async Task<TransferResponse> TransferAsync([FromBody] TransferRequest source)
        {
            var reqserver = new Model.Game.OB.Reqserver.TransferReqserver
            {
                transferNo = source.TransferNo,
                loginName = source.loginName,
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString().PadRight(13, '0')
            };
            return await _iobapiservice.TransferAsync(reqserver);
        }
        /// <summary>
        /// 取得明細
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("betHistoryRecord")]
        public async Task<BetHistoryRecordResponse> BetHistoryRecordAsync([FromBody] BetHistoryRecordRequest source)
        {
            var reqserver = new Model.Game.OB.Reqserver.BetHistoryRecordReqserver
            {
                startTime=source.startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime=source.endTime.AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                pageIndex=source.pageIndex,
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
            };
            return await _iobapiservice.BetHistoryRecordAsync(reqserver);
        }
        /// <summary>
        /// 取得每日報表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("reportAgent")]
        public async Task<ReportAgentResponse> ReportAgentAsync([FromBody] ReportAgentRequest source)
        {
            var reqserver = new Model.Game.OB.Reqserver.ReportAgentReqserver
            {
                startDate = source.startDate.ToString("yyyyMMdd"),
                endDate = source.endDate.ToString("yyyyMMdd"),
                pageIndex = source.pageIndex,
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
            };
            return await _iobapiservice.ReportAgentAsync(reqserver);
        }

        [HttpPost]
        [Route("onlineUsers")]
        public async Task<ReportAgentResponse> OnlineUsersAsync([FromBody] OnlineUsersRequest source)
        {
            var reqserver = new Model.Game.OB.Reqserver.OnlineUsersReqserver
            {
                pageIndex = source.pageIndex,
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
            };
            return await _iobapiservice.OnlineUsersAsync(reqserver);
        }

    }
}
