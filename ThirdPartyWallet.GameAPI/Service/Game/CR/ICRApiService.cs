using ThirdPartyWallet.Share.Model.Game.CR.Request;
using ThirdPartyWallet.Share.Model.Game.CR.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.CR
{
    public interface ICRApiService
    {
        Task<AGLoginResponse> AGLoginAsync(AGLoginRequest source);

        Task<CreateMemberResponse> CreateMemberAsync(CreateMemberRequest source);
        Task<DepositResponse> DepositAsync(DepositRequest source);

        Task<WithdrawResponse> WithdrawAsync(WithdrawRequest source);

        Task<MemLoginResponse> MemLoginAsync(MemLoginRequest source);

        Task<LaunchGameResponse> LaunchGameAsync(LaunchGameRequest source);

        Task<chkMemberBalanceResponse> chkMemberBalanceAsync(chkMemberBalanceRequest source);

        Task<ALLWagerResponse> ALLWagerAsync(ALLWagerRequest source, int retry = -1);

        Task<KickOutMemResponse> KickOutMemAsync(KickOutMemRequest source);

        Task<ChkTransInfoResponse> ChkTransInfoAsync(ChkTransInfoRequest source);


        Task<CheckAGReportResponse> CheckAGReportAsync(CheckAGReportRequest source);

        Task<string> healthcheckAsync();
    }
}