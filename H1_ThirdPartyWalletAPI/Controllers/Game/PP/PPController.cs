
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.PP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PP.Responses;
using H1_ThirdPartyWalletAPI.Service.Game.PP;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.Game.PP.Responses.PP_Responses;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.PP
{
    [Route("/[controller]")]
    [ApiController]
    public class PPController : ControllerBase
    {
        private readonly IPPApiService _ippapiservice;
        public PPController(IPPApiService ippapiservice)
        {

            _ippapiservice = ippapiservice;
        }
        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateMember")]
        public async Task<PP_Responses> CreateMemberAsync([FromBody] CreatePlayerRequest source)
        {
            source.secureLogin = Config.CompanyToken.PP_SecureLogin;
            return await _ippapiservice.CreateMemberAsync(source);
        }

        /// <summary>
        /// 取得遊戲URL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("StartGame")]
        public async Task<StartGameResponses> StartGameAsync([FromBody] StartGameRequest source)
        {
            source.secureLogin = Config.CompanyToken.PP_SecureLogin;
            return await _ippapiservice.StartGameAsync(source);
        }
        /// <summary>
        /// 存款取款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Transfer")]
        public async Task<TransferResponses> TransferAsync([FromBody] TransferRequest source)
        {
            source.secureLogin = Config.CompanyToken.PP_SecureLogin;
            source.externalTransactionId = Guid.NewGuid().ToString();
            return await _ippapiservice.TransferAsync(source);
        }
        /// <summary>
        /// 取得會員額度
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetBalance")]
        public async Task<GetBalanceResponses> GetBalanceAsync([FromBody] GetBalanceRequest source)
        {
            source.secureLogin = Config.CompanyToken.PP_SecureLogin;
            return await _ippapiservice.GetBalanceAsync(source);
        }

        /// <summary>
        /// 取得交易紀錄狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTransferStatus")]
        public async Task<GetTransferStatusResponses> GetTransferStatusAsync([FromBody] GetTransferStatusRequest source)
        {
            source.secureLogin = Config.CompanyToken.PP_SecureLogin;
            return await _ippapiservice.GetTransferStatusAsync(source);
        }
        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TerminateSession")]
        public async Task<TerminateSessionResponses> TerminateSessionAsync([FromBody] TerminateSessionRequest source)
        {
            source.secureLogin = Config.CompanyToken.PP_SecureLogin;
            return await _ippapiservice.TerminateSessionAsync(source);
        }

        /// <summary>
        /// 取得注單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetRecord")]
        public async Task<List<GetRecordResponses>> GetRecordAsync([FromBody] GetRecordRequest source)
        {
            source.login = Config.CompanyToken.PP_SecureLogin;
            source.password = Config.CompanyToken.PP_Key;
            source.timepoint = (DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeMilliseconds().ToString());
            return await _ippapiservice.GetRecordAsync(source);
        }
        /// <summary>
        /// 注單轉跳URL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("OpenHistory")]
        public async Task<OpenHistoryResponses> OpenHistoryAsync([FromBody] OpenHistoryRequest source)
        {
            source.secureLogin = Config.CompanyToken.PP_SecureLogin;
            return await _ippapiservice.OpenHistoryAsync(source);
        }

        [HttpPost]
        [Route("HealthCheck")]
        public async Task<HealthCheckResponse> HealthCheckAsync()
        {
            return await _ippapiservice.HealthCheckAsync();
        }


        /// <summary>
        /// 取得注單URL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Environment")]
        public async Task<EnvironmentResponses> EnvironmentAsync([FromBody] EnvironmentRequest source)
        {
            source.secureLogin = Config.CompanyToken.PP_SecureLogin;
            return await _ippapiservice.EnvironmentAsync(source);
        }
    }
}
