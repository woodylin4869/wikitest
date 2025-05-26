using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.SEXY
{
    public interface ISEXYApiService
    {
        Task<CreateMemberResponse> CreateMember(CreateMemberRequest source);

        Task<GetBalanceResponse> GetBalance(GetBalanceRequest source);

        Task<GameLoginResponse> GameLogin(GameLoginRequest source);

        Task<DoLoginAndLaunchGameResponse> DoLoginAndLaunchGame(DoLoginAndLaunchGameRequest source);

        Task<UpdateBetLimitResponse> UpdateBetLimit(UpdateBetLimitRequest source);

        Task<GameLogoutResponse> GameLogout(GameLogoutRequest source);

        //Task<GetGameListResponse> GetGameTableList(GetGameListRequest source);

        Task<DepositResponse> Deposit(DepositRequest source);

        Task<WithdrawResponse> Withdraw(WithdrawRequest source);

        Task<CheckTransferOperationResponse> CheckTransferOperation(CheckTransferOperationRequest source);

        Task<GetTransactionByUpdateDateResponse> GetTransactionByUpdateDate(GetTransactionByUpdateDateRequest source);

        Task<GetTransactionByTxTimeResponse> GetTransactionByTxTime(GetTransactionByTxTimeRequest source);

        Task<GetSummaryByTxTimeHourResponse> GetSummaryByTxTimeHour(GetSummaryByTxTimeHourRequest source);

        Task<GetTransactionHistoryResultResponse> GetTransactionHistoryResult(GetTransactionHistoryResultRequest source);
        
    }
}
