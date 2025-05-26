using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Interface
{
    public interface IJDBApiService
    {
        Task<GetTokenResponse> Action11_GetToken(GetTokenRequest request);
        Task<ResponseBaseModel> Action12_CreatePlayer(CreatePlayerRequest request);
        Task<QueryPlayerResponse> Action15_QueryPlayer(QueryPlayerRequest request);
        Task<ResponseBaseModel> Action17_KickOut(KickOutRequest request);
        Task<DepositOrWithdrawResponse> Action19_DepositOrWithdraw(DepositOrWithdrawRequest request);
        Task<BetRecordCollection> Action29_GetGameBetRecord(GetGameBetRecordRequest request);
        Task<GetBetRecordResponse> Action29_GetGameBetRecord_NoClassification(GetGameBetRecordRequest request);
        Task<GetDailyReportRepsonse> Action42_DailyReport(GetDailyReportRequest request);
        Task<ResponseBaseModel> Action43_JackpotContribution(JackpotContributionRequest request);
        Task<GetGameListResponse> Action49_GetGameList(GetGameListRequest request);
        Task<GetInGamePlayerResponse> Action52_GetInGamePlayer(GetInGamePlayerRequest request);
        Task<GetGameResultResponse> Action54_GetGameResult(GetGameResultRequest request);
        Task<GetCashTransferRecordResponse> Action55_GetCashTransferRecord(GetCashTransferRecordRequest request);
        Task<ResponseBaseModel> Action58_KickOutOfflineUsers(KickoutOfflineUsersRequest request);
        Task<BetRecordCollection> Action64_GetGameHistory(GetGameHistoryRequest request);
        Task<GetBetRecordResponse> Action64_NoClassification(GetGameHistoryRequest request);
        Task<GetOnlineUserResponse> Action65_GetOnlineUser(GetOnlineUserRequest request);
    }
}