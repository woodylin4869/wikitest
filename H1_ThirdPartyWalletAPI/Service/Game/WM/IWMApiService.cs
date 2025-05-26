using H1_ThirdPartyWalletAPI.Model.Game.WM.Request;
using H1_ThirdPartyWalletAPI.Model.Game.WM.Response;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.WM
{
    public interface IWMApiService
    {
        Task<WMResponse> MemberRegisterAsync(MemberRegisterRequest source);

        Task<WMResponse> SigninGameAsync(SigninGameRequest source);

        Task<WMResponse> LogoutGameAsync(LogoutGameRequest source);

        Task<WMResponse> GetBalanceAsync(GetBalanceRequest source);

        Task<WMBalanceResponse> ChangeBalanceAsync(ChangeBalanceRequest source);

        Task<WMTradeResponse> GetMemberTradeReportAsync(GetMemberTradeReportRequest source);

        Task<WMDataReportResponse> GetDateTimeReportAsync(GetDateTimeReportRequest source);

        Task<WMResponse> EditLimitAsync(EditLimitRequest source);

        Task<HelloResponse> HelloAsync(HelloRequest source);
    }
}
