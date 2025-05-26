using H1_ThirdPartyWalletAPI.Model.Game.OB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Response;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.OB
{
    public interface IOBApiService
    {
        Task<CreateMemberResponse> CreateMemberAsync(Model.Game.OB.Reqserver.CreateMemberRequest source);

        Task<FastGameResponse> FastGameAsync( Model.Game.OB.Reqserver.FastGameReqserver source);

        Task<GetbalanceResponse> GetbalanceAsync(Model.Game.OB.Reqserver.GetbalanceReqserver source);

        Task<DepositResponse> DepositAsync(Model.Game.OB.Reqserver.depositReqserver source);

        Task<WithdrawResponse> WithdrawAsync(Model.Game.OB.Reqserver.WithdrawReqserver source);

        Task<TransferResponse> TransferAsync(Model.Game.OB.Reqserver.TransferReqserver source);

        Task<BetHistoryRecordResponse> BetHistoryRecordAsync(Model.Game.OB.Reqserver.BetHistoryRecordReqserver source);

        Task<ReportAgentResponse> ReportAgentAsync(Model.Game.OB.Reqserver.ReportAgentReqserver source);
        Task<ReportAgentResponse> OnlineUsersAsync(Model.Game.OB.Reqserver.OnlineUsersReqserver source);


    }
}
