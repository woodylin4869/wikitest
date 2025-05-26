using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Request;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.RGRICH
{
    public interface IRGRICHApiService
    {
        Task<ResponseBase<CreateUserResponse>> CreateUserAsync(CreateUserRequest source, string lang = "en");

        Task<ResponseBase<UserIsExistResponse>> UserIsExistAsync(UserIsExistDataRequest source, string lang = "en");

        Task<ResponseBase<BalanceResponse>> BalanceAsync(BalanceRequest source, string lang = "en");

        Task<ResponseBase<RechargeResponse>> RechargeAsync(RechargeRequest source, string lang = "en");

        Task<ResponseBase<WithdrawResponse>> WithdrawAsync(WithdrawRequest source, string lang = "en");

        Task<ResponseBase<RechargeOrWithdrawRecordResponse>> RechargeOrWithdrawRecordAsync(RechargeOrWithdrawRecordRequest source, string lang = "en");

        Task<ResponseBase<GameUrlResponse>> GameUrlAsync(GameUrlRequest source, string lang = "en");

        Task<ResponseBaseWithMeta<List<BetRecordResponse>>> BetRecordAsync(BetRecordRequest source, string lang = "en");

        Task<ResponseBase<BetDetailUrlResponse>> BetDetailUrlAsync(BetDetailUrlRequest source, string lang = "en");

        Task<ResponseBase<Dictionary<string, string>>> GameListAsync(GameListRequest source, string lang = "en");

        Task<ResponseBase<ReportHourResponse>> ReportHourAsync(ReportHourRequest source, string lang = "en");

        Task<ResponseBase<object>> KickUserAsync(KickUserRequest source, string lang = "en");

        Task<HealthCheckResponse> HealthCheckAsync(HealthCheckRequest source, string lang = "en");
    }
}