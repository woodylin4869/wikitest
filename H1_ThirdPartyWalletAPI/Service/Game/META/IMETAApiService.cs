using H1_ThirdPartyWalletAPI.Model.Game.META.Request;
using H1_ThirdPartyWalletAPI.Model.Game.META.Response;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.META
{
    public interface IMETAApiService
    {
        Task<CreateMemberResponse> CreateMember(CreateMemberRequest source);

        Task<CheckPointResponse> CheckPoint(CheckPointRequest source);

        Task<GameLoginResponse> GameLogin(GameLoginRequest source);

        Task<GameLogoutResponse> GameLogout(GameLogoutRequest source);
        
        Task<GetGameListResponse> GetGameTableList(GetGameListRequest source);

        Task<TransPointResponse> TransPoint(TransPointRequest source);

        Task<TransactionLogResponse> TransactionLog(TransactionLogRequest source);

        Task<BetOrderRecordResponse> BetOrderRecord(BetOrderRecordRequest source);
        
    }
}
