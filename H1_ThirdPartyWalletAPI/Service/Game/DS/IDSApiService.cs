using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Request;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Response;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model;

namespace H1_ThirdPartyWalletAPI.Service.Game.DS
{
    public interface IDSApiService
    {
        Task<CheckBalanceResponse> checkBalance(CheckBalanceRequest request);
        Task<CreateMemberRepsonse> CreateMember(CreateMemberRequest request);
        Task<GetBetDetailPageResponse> GetBetDetailPage(GetBetDetailPageRequest request);
        Task<GetBetHistoryResponse> GetBetHistoryByPlayer(GetBetHistoryRequest request);
        Task<GetBetRecordResponse> GetBetRecord(GetBetRecordRequest request);
        Task<GetGameInfoStateListResponse> GetGameInfoStateList();
        Task<GetLoginInfoResponse> GetLoginInfo(GetLoginInfoRequest request);
        Task<GetMemberBetRecordByHourResponse> GetMemberBetRecordByHour(GetMemberBetRecordByHourRequest request);
        /// <summary>
        /// 代理下注紀錄匯總
        /// 1小時內最多執行4次，每次查詢區間最長為24小時
        /// 最久能查到的時間為90天前到現在
        /// 資料有兩小時延遲(最快能看到的統計資料= 現在時間-2h)
        /// 最小查詢的時間單位為小時，ex: 00:00:00 ~ 23:59:59或 01:00:00 ~ 01:59:59
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GetAgentSummaryBetRecordsResponse> GetAgentSummaryBetRecords(GetAgentSummaryBetRecordsRequest request);
        Task<GetOnlineMemberBalanceResponse> GetOnlineMemberBalance(GetOnlineMemberBalanceRequest request);
        Task<GetOnlineMembersResponse> GetOnlineMembers();
        Task<LoginGameResponse> LoginGame(LoginGameRequest request);
        Task<LogoutResponse> Logout(LogoutRequest request);
        Task<TransferResponse> Transfer(TransferRequest request);
        Task<VerifyResponse> verify(VerifyRequest request);
    }
}