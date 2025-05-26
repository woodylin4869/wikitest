using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.GR.Request;
using H1_ThirdPartyWalletAPI.Model.Game.GR.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Game.GR;
using H1_ThirdPartyWalletAPI.Model.Config;
using System.Linq;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.GR
{
    /// <summary>
    /// GR API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class GRController : ControllerBase
    {
        private readonly IGRApiService _GRApiService;

        public GRController(IGRApiService GRApiService)
        {
            _GRApiService = GRApiService;
        }

        /// <summary>
        /// 0001 – 平台確認使用者是否在線上 check_user_online
        /// </summary>
        [HttpPost]
        [Route("CheckUserOnline")]
        public async Task<CheckUserOnlineResponse> CheckUserOnline([FromBody] CheckUserOnlineRequest source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.CheckUserOnline(source);
        }

        /// <summary>
        /// 0002-v3 - 平台使用者轉入點數 credit_balance_v3
        /// </summary>
        [HttpPost]
        [Route("CreditBalanceV3")]
        public async Task<CreditBalanceV3Response> CreditBalanceV3([FromBody] CreditBalanceV3Request source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.CreditBalanceV3(source);
        }

        /// <summary>
        /// 0003-v3 - 平台使用者轉出點數 debit_balance_v3
        /// </summary>
        [HttpPost]
        [Route("DebitBalanceV3")]
        public async Task<DebitBalanceV3Response> DebitBalanceV3(DebitBalanceV3Request source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.DebitBalanceV3(source);
        }

        /// <summary>
        /// 0018 – 平台檢查是否已有單號存在 check_order_exist_v3
        /// </summary>
        [HttpPost]
        [Route("CheckOrderExistV3")]
        public async Task<CheckOrderExistV3Response> CheckOrderExistV3(CheckOrderExistV3Request source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.CheckOrderExistV3(source);
        }

        /// <summary>
        /// 0004 – 平台註冊使用者 reg_user_info
        /// </summary>
        [HttpPost]
        [Route("RegUserInfo")]
        public async Task<RegUserInfoResponse> RegUserInfo(RegUserInfoRequest source)
        {
            return await _GRApiService.RegUserInfo(source);
        }

        /// <summary>
        /// 0005 - 平台踢出使用者 kick_user_by_account
        /// </summary>
        [HttpPost]
        [Route("KickUserByAccount")]
        public async Task<KickUserByAccountResponse> KickUserByAccount(KickUserByAccountRequest source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.KickUserByAccount(source);
        }

        /// <summary>
        /// 0006-2 - 平台取得 Slot 使用者下注歷史資料 get_slot_all_bet_details
        /// </summary>
        [HttpPost]
        [Route("GetSlotAllBetDetails")]
        public async Task<CommBetDetailsResponse> GetSlotAllBetDetails(CommBetDetailsRequest source)
        {
            source.account = null;
            //source.start_time = null;
            //source.end_time = null;
            //source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.GetSlotAllBetDetails(source);
        }

        /// <summary>
        /// 0007-2 - 平台取得 Slot 下注遊戲後詳細資訊的結果 get_slot_game_round_details
        /// </summary>
        [HttpPost]
        [Route("GetSlotGameRoundDetails")]
        public async Task<GetSlotGameRoundDetailsResponse> GetSlotGameRoundDetails(GetSlotGameRoundDetailsRequest source)
        {
            return await _GRApiService.GetSlotGameRoundDetails(source);
        }

        /// <summary>
        /// 0006-3 - 平台取得魚機使用者下注歷史資料 get_fish_all_bet_details
        /// </summary>
        [HttpPost]
        [Route("GetFishAllBetDetails")]
        public async Task<CommBetDetailsResponse> GetFishAllBetDetails(CommBetDetailsRequest source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.GetFishAllBetDetails(source);
        }

        /// <summary>
        /// 0007-3 - 平台取得魚機遊戲結算後詳細資訊 get_fish_game_round_details
        /// </summary>
        [HttpPost]
        [Route("GetFishGameRoundDetails")]
        public async Task<GetFishGameRoundDetailsResponse> GetFishGameRoundDetails(GetFishGameRoundDetailsRequest source)
        {
            return await _GRApiService.GetFishGameRoundDetails(source);
        }

        /// <summary>
        /// 0008 – 平台取得交易詳細記錄 get_transaction_details
        /// </summary>
        [HttpPost]
        [Route("GetTransactionDetails")]
        public async Task<GetTransactionDetailsResponse> GetTransactionDetails(GetTransactionDetailsRequest source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.GetTransactionDetails(source);
        }

        /// <summary>
        /// 0009 –平台取得所有在線遊戲有效投注總額 get_user_bet_amount
        /// </summary>
        [HttpPost]
        [Route("GetUserBetAmount")]
        public async Task<GetUserBetAmountResponse> GetUserBetAmount(GetUserBetAmountRequest source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.GetUserBetAmount(source);
        }

        /// <summary>
        /// 0010 – 平台取得使用者輸贏金額 get_user_win_or_lost
        /// </summary>
        [HttpPost]
        [Route("GetUserWinOrLost")]
        public async Task<GetUserWinOrLostResponse> GetUserWinOrLost(GetUserWinOrLostRequest source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.GetUserWinOrLost(source);
        }

        /// <summary>
        /// 0013 – 平台取得使用者登入 (sid) get_sid_by_account
        /// </summary>
        [HttpPost]
        [Route("GetSidByAccount")]
        public async Task<GetSidByAccountResponse> GetSidByAccount(GetSidByAccountRequest source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.GetSidByAccount(source);
        }

        /// <summary>
        /// 0014 – 平台使用者取得餘額 get_balance
        /// </summary>
        [HttpPost]
        [Route("GetBalance")]
        public async Task<GetBalanceResponse> GetBalance(GetBalanceRequest source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.GetBalance(source);
        }

        /// <summary>
        /// 0017 – 平台確認使用者是否存在 check_user_exist
        /// </summary>
        [HttpPost]
        [Route("CheckUserExist")]
        public async Task<CheckUserExistResponse> CheckUserExist(CheckUserExistRequest source)
        {
            source.account = source.account + "@" + Config.CompanyToken.GR_Site_Code;
            return await _GRApiService.CheckUserExist(source);
        }

        /// <summary>
        /// 0020 – 平台取得代理額度 get_agent_detail
        /// </summary>
        [HttpPost]
        [Route("GetAgentDetail")]
        public async Task<GetAgentDetailResponse> GetAgentDetail(GetAgentDetailRequest source)
        {
            return await _GRApiService.GetAgentDetail(source);
        }

        /// <summary>
        /// 0021 – 平台取得代理遊戲列表 get_agent_game_list
        /// </summary>
        [HttpPost]
        [Route("GetAgentGameList")]
        public async Task<GetAgentGameListResponse> GetAgentGameList(GetAgentGameListRequest source)
        {
            return await _GRApiService.GetAgentGameList(source);
        }
    }
}
