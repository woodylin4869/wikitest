using System.Collections.Generic;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.RTG
{
    /// <summary>
    /// RTG API
    /// </summary>
    public interface IRTGApiService
    {
        Task<GetGameResponse> GetGame(GetGameRequest source);
        Task<CreateUpdateMemberResponse> CreateUpdateMember(CreateUpdateMemberRequest source);
        Task<GetUserResponse> GetUser(GetUserRequest source);
        Task<GetGameUrlResponse> GetGameUrl(GetGameUrlRequest source);
        Task<DepositResponse> Deposit(DepositRequest source);
        Task<WithdrawResponse> Withdraw(WithdrawRequest source);
        Task<SingleTransactionResponse> SingleTransaction(SingleTransactionRequest source);
        Task<KickUserResponse> KickUser(KickUserRequest source);
        Task<KickAllResponse> KickAll(KickAllRequest source);
        Task<GetOnlineUserResponse> GetOnlineUser(GetOnlineUserRequest source);
        Task<GameSettlementRecordResponse> GameSettlementRecord(GameSettlementRecordRequest source);
        Task<GetGameDailyRecordResponse> GetGameDailyRecord(GetGameDailyRecordRequest source);
        Task<GetVideoLinkResponse> GetVideoLink(GetVideoLinkRequest source);
    }
}