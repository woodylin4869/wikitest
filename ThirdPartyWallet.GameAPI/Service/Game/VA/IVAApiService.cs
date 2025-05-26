using ThirdPartyWallet.Share.Model.Game.VA.Request;
using ThirdPartyWallet.Share.Model.Game.VA.Response;

namespace ThirdPartyWallet.GameAPI.Service.Game.VA
{
    public interface IVAApiService
    {
        Task<BaseResponse<CreateResponse>> CreateAsync(CreateRequest source);


        Task<BaseResponse<GetBalanceResponse>> GetBalanceAsync(GetBalanceRequest source);

        Task<BaseResponse<KickUserResponse>> KickUserAsync(KickUserRequest source);

        Task<BaseResponse<GetGameListResponse>> GetGameListAsync(GetGameListRequest source);

        Task<BaseResponse<GameLinkResponse>> GameLinkAsync(GameLinkRequest source);

        Task<BaseResponse<DepositResponse>> DepositAsync(DepositRequest source);

        Task<BaseResponse<WithdrawResponse>> WithdrawAsync(WithdrawRequest source);

        Task<BaseResponse<TransactionDetailResponse>> TransactionDetailAsync(TransactionDetailRequest source);
        Task<BaseResponse<BetlogListByTimeResponse>> BetlogListByTimeAsync(BetlogListByTimeRequest source);
        Task<BaseResponse<BetlogDetailResponse>> BetlogDetailAsync(BetlogDetailRequest source);
        Task<BaseResponse<BetlogHistoryListByTimeResponse>> BetlogHistoryListByTimeAsync(BetlogHistoryListByTimeRequest source);
        Task<BaseResponse<ReportCurrencyResponse>> ReportCurrencyAsync(ReportCurrencyRequest source);
        Task<healthcheckResponse> healthcheckAsync(healthcheckRequest source);
    }
}
