using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.WE.Request;
using ThirdPartyWallet.Share.Model.Game.WE.Response;

namespace ThirdPartyWallet.GameAPI.Service.Game.WE;

public interface IWEApiService
{
    Task<CreateUserResponse> CreateUserAsync(CreateUserRequest source);
    Task<DepositResponse> DepositAsync(DepositRequest source);
    Task<WithdrawResponse> WithdrawAsync(WithdrawRequest source);
    Task<BalanceResponse> BalanceAsync(BalanceRequest source);
    Task<BetlimitResponse> BetlimitAsync(BetlimitRequest source);
    Task<List<GameListResponse.Datum>> GameListAsync(GameListRequest source);
    Task<LoginResponse> LoginAsync(LoginRequest source);
    Task<LogoutResponse> LogoutAsync(LogoutRequest source);
    Task<LogoutAllResponse> LogoutAllAsync(LogoutAllRequest source);
    Task<TransferResponse> TransferAsync(TransferRequest source);
    Task<BetRecordResponse> BetRecordAsync(BetRecordRequest source);
    Task<ReportHourResponse> ReportHourAsync(ReportHourRequest source);
    Task<BetDetailUrlResponse> BetDetailUrlAsync(BetDetailUrlRequest source);
    Task<dynamic> HealthCheckAsync(HealthCheckRequest source);

    Task<SetbetlimitResponse> SetBetLimitAsync(SetbetlimitRequest source);

}
