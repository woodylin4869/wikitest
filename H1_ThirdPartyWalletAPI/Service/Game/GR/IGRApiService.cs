using System.Collections.Generic;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.GR.Request;
using H1_ThirdPartyWalletAPI.Model.Game.GR.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.GR
{
    /// <summary>
    /// GR API
    /// </summary>
    public interface IGRApiService
    {
        Task<CheckUserOnlineResponse> CheckUserOnline(CheckUserOnlineRequest source);
        Task<CreditBalanceV3Response> CreditBalanceV3(CreditBalanceV3Request source);
        Task<DebitBalanceV3Response> DebitBalanceV3(DebitBalanceV3Request source);
        Task<CheckOrderExistV3Response> CheckOrderExistV3(CheckOrderExistV3Request source);
        Task<RegUserInfoResponse> RegUserInfo(RegUserInfoRequest source);
        Task<KickUserByAccountResponse> KickUserByAccount(KickUserByAccountRequest source);
        Task<CommBetDetailsResponse> GetSlotAllBetDetails(CommBetDetailsRequest source);
        Task<GetSlotGameRoundDetailsResponse> GetSlotGameRoundDetails(GetSlotGameRoundDetailsRequest source);
        Task<CommBetDetailsResponse> GetFishAllBetDetails(CommBetDetailsRequest source);
        Task<GetFishGameRoundDetailsResponse> GetFishGameRoundDetails(GetFishGameRoundDetailsRequest source);
        Task<GetTransactionDetailsResponse> GetTransactionDetails(GetTransactionDetailsRequest source);
        Task<GetUserBetAmountResponse> GetUserBetAmount(GetUserBetAmountRequest source);
        Task<GetUserWinOrLostResponse> GetUserWinOrLost(GetUserWinOrLostRequest source);
        Task<GetSidByAccountResponse> GetSidByAccount(GetSidByAccountRequest source);
        Task<GetBalanceResponse> GetBalance(GetBalanceRequest source);
        Task<CheckUserExistResponse> CheckUserExist(CheckUserExistRequest source);
        Task<GetAgentDetailResponse> GetAgentDetail(GetAgentDetailRequest source);
        Task<GetAgentGameListResponse> GetAgentGameList(GetAgentGameListRequest source);
        Task<GetreportResponse> GetReportList(GetReportRequest source);
        Task<GetUrlResponse> GetURLList(GetUrlRequest source);
    }
}
