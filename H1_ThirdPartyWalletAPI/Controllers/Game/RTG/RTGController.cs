using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Game.RTG;
using H1_ThirdPartyWalletAPI.Model.Config;
using System.Linq;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.RTG
{
    /// <summary>
    /// RTG API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class RTGController : ControllerBase
    {
        private readonly IRTGApiService _rtgApiService;

        public RTGController(IRTGApiService rtgApiService)
        {
            _rtgApiService = rtgApiService;
        }


        /// <summary>
        /// 取得遊戲列表
        /// </summary>
        [HttpPost]
        [Route("GetGame")]
        public async Task<GetGameResponse> GetGameAsync([FromBody] GetGameRequest source)
        {
            return await _rtgApiService.GetGame(source);
        }

        
        /// <summary>
        /// 建立會員(幣別帳戶) 
        /// </summary>
        [HttpPost]
        [Route("CreateUpdateMember")]
        public async Task<CreateUpdateMemberResponse> CreatePlayerAsync([FromBody] CreateUpdateMemberRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;
            return await _rtgApiService.CreateUpdateMember(source);
        }

        /// <summary>
        /// 取得玩家訊息
        /// </summary>
        [HttpPost]
        [Route("GetUser")]
        public async Task<GetUserResponse> GetUserAsync([FromBody] GetUserRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;
            return await _rtgApiService.GetUser(source);
        }

        /// <summary>
        /// 取得遊戲連結
        /// </summary>
        [HttpPost]
        [Route("GetGameUrl")]
        public async Task<GetGameUrlResponse> GetGameUrlAsync([FromBody] GetGameUrlRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;
            return await _rtgApiService.GetGameUrl(source);
        }

       
        /// <summary>
        /// 存入點數 
        /// </summary>
        [HttpPost]
        [Route("Deposit")]
        public async Task<DepositResponse> DepositAsync([FromBody] DepositRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;
            return await _rtgApiService.Deposit(source);
        }
        /// <summary>
        /// 取出點數 
        /// </summary>
        [HttpPost]
        [Route("Withdraw")]
        public async Task<WithdrawResponse> WithdrawAsync([FromBody] WithdrawRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;
            return await _rtgApiService.Withdraw(source);
        }

        /// <summary>
        /// 查詢點數交易結果
        /// </summary>
        [HttpPost]
        [Route("SingleTransaction")]
        public async Task<SingleTransactionResponse> SingleTransactionAsync([FromBody] SingleTransactionRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;
            return await _rtgApiService.SingleTransaction(source);
        }
        /// <summary>
        /// 踢人
        /// </summary>
        [HttpPost]
        [Route("KickUser")]
        public async Task<KickUserResponse> KickUserAsync([FromBody] KickUserRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;
            return await _rtgApiService.KickUser(source);
        }
        /// <summary>
        /// 全踢
        /// </summary>
        [HttpPost]
        [Route("KickAll")]
        public async Task<KickAllResponse> KickAllAsync([FromBody] KickAllRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;
            return await _rtgApiService.KickAll(source);
        }
        /// <summary>
        /// 取得遊戲中的會員
        /// </summary>
        [HttpPost]
        [Route("GetOnlineUser")]
        public async Task<GetOnlineUserResponse> GetOnlineUserAsync([FromBody] GetOnlineUserRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;

            var gamelist = Model.Game.RTG.RTG.GameList.Keys.ToList();


            return await _rtgApiService.GetOnlineUser(source);
        }
        /// <summary>
        /// 取得遊戲帳務
        /// </summary>
        [HttpPost]
        [Route("GameSettlementRecord")]
        public async Task<GameSettlementRecordResponse> GameSettlementRecordAsync([FromBody] GameSettlementRecordRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;
            return await _rtgApiService.GameSettlementRecord(source);
        }
        /// <summary>
        /// 取得遊戲每日統計資訊
        /// </summary>
        [HttpPost]
        [Route("GetGameDailyRecord")]
        public async Task<GetGameDailyRecordResponse> GetGameDailyRecordAsync([FromBody] GetGameDailyRecordRequest source)
        {
            source.SystemCode = Config.CompanyToken.RTG_SystemCode;
            source.WebId = Config.CompanyToken.RTG_WebID;
            return await _rtgApiService.GetGameDailyRecord(source);
        }
    }
}
