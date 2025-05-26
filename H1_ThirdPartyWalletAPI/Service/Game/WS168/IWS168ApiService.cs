using H1_ThirdPartyWalletAPI.Model.Game.WS168.Request;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Response;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.WS168
{
    public interface IWS168ApiService
    {
        Task<CreatePlayerResponse> CreatePlayerAsync(CreatePlayerRequest source);

        Task<PlayerLoginResponse> PlayerLoginAsync(PlayerLoginRequest source);

        Task<DepositResponse> DepositAsync( DepositRequest source);

        Task<WithdrawResponse> WithdrawAsync(WithdrawRequest source);

        Task<QueryPlayerBalanceResponse> QueryPlayerBalanceAsync(QueryPlayerBalanceRequest source);

        Task<SearchingOrdersStatusResponse> SearchingOrdersStatusAsync(SearchingOrdersStatusRequest source);

        Task<SearchingOrdersStatusResponse> BetLogAsync(BetLogRequest source);
        Task<PlayerLogoutResponse> PlayerLogoutAsync(PlayerLogoutRequest source);
    }
}
