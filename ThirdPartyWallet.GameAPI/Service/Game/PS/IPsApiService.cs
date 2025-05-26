using ThirdPartyWallet.Share.Model.Game.PS.Request;
using ThirdPartyWallet.Share.Model.Game.PS.Response;

namespace ThirdPartyWallet.GameAPI.Service.Game.PS
{
    public interface IPsApiService
    {
        Task<CreateuserResponse> CreateplayerAsync(CreateuserRequest source);
        Task<GetgameResponse> GetgmaeurlAsync(GetgameRequest source);

        Task<DepositResponse> MoneyinAsync(DepositRequest source);

        Task<WithdrawResponse> MoneyoutAsync(WithdrawRequest source);

        Task<GetbalanceResponse> GetBalanceAsync(GetbalanceRequest source);
        Task<kickoutResponse> KickUserAsync(kickoutRequest source);
        Task<kickallResponse> KickallAsync(kickallRequest source);
        Task<List<QueryorderResponse>> QueryorderAsync(QueryorderRequest source);
        Task<healthcheckResponse> healthcheckAsync(healthcheckRequest source);
        Task<hoursummaryResponse> hourlysummaryAsync(hoursummaryRequest source);
        Task<Dictionary<string, Dictionary<string, List<GetorderResponse.BetRecord>>>> gamehistoryAsync(GetorderRequest source);
    }
}
