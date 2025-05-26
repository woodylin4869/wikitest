
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.JILI
{
    public interface IJILIApiService
    {
        Task<CreateMemberResponse> CreateMemberAsync(CreateMemberRequest source);

        Task<LoginWithoutRedirectResponse> LoginWithoutRedirectAsync(LoginWithoutRedirectRequest source);

        Task<GetGameListResponse> GetGameListAsync();

        Task<GetMemberInfoResponse> GetMemberInfoAsync(GetMemberInfoRequest source);

        Task<ExchangeTransferByAgentIdResponse> ExchangeTransferByAgentIdAsync(ExchangeTransferByAgentIdRequest source);
        Task<KickMemberResponses> KickMemberAsync(KickMemberRequest source);
        Task<GetBetRecordByTimeResponse> GetBetRecordByTimeAsync(GetBetRecordByTimeRequest source);
        Task<GetBetRecordSummaryResponse> GetBetRecordSummaryAsync(GetBetRecordSummaryRequest source);
        Task<GetGameDetailUrlResponse> GetGameDetailUrlAsync(GetGameDetailUrlRequest source);
        Task<CheckTransferByTransactionIdResponse> CheckTransferByTransactionIdAsync(CheckTransferByTransactionIdRequest source);
        Task<GetOnlineMemberResponse> GetOnlineMemberAsync();
    }
}
