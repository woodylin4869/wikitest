using ThirdPartyWallet.Share.Model.Game.IDN.Request;
using ThirdPartyWallet.Share.Model.Game.IDN.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.IDN
{
    public interface IIDNApiService
    {
        Task<AuthResponse> AuthAsync(AuthRequest source);

        Task<ResponseBase<RegistrationResponse>> RegistrationAsync(RegistrationRequest source);

        Task<ResponseBase<UserAuthResponse>> UserAuthAsync(UserAuthRequest source);
        Task<ResponseBase<UserIsExistResponse>> UserIsExistAsync(UserIsExistDataRequest source);

        Task<ResponseBase<BalanceResponse>> BalanceAsync(BalanceRequest source);

        Task<ResponseBase<object>> CalibrateAsync(CalibrateRequest source);

        Task<ResponseBase<WithdrawResponse>> WithdrawAsync(string UserName, WithdrawRequest source);

        Task<ResponseBase<DepositResponse>> DepositAsync(string UserName, DepositRequest source);

        Task<ResponseBase<CheckDepositListResponse>> CheckDepositListAsync(int page, CheckDepositListRequest source);

        Task<ResponseBase<CheckWithdrawListResponse>> CheckWithdrawListAsync(int page, CheckWithdrawListRequest source);


        Task<ResponseBase<bethistoryResponse>> bethistoryAsync(bethistoryRequest source);

        Task<ResponseBase<dailyreportResponse>> dailyreportAsync(dailyreportRequest source);

        Task<ResponseBase<WhitelabelInfoResponse>> HealthCheckAsync();

        Task<ResponseBase<LaunchResponse>> LaunchAsync(string UserName, string GameID, LaunchRequest source);
        Task<LogoutResponse> LogoutAsync(string UserName, LogoutRequest source);


        void SetAuthToken(string access_token);
        Task<GetGameResultResponse> GetGameResultAsync(GetGameResultRequest source);
    }
}