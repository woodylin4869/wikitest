using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.Gemini.Request;
using ThirdPartyWallet.Share.Model.Game.Gemini.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.Gemini
{
    public interface IGeminiApiService
    {
        Task<CreateplayerResponse> CreateplayerAsync(CreateplayerRequest source);
        Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest source);

        Task<TransferinResponse> TransferinAsync(TransferinRequest source);

        Task<TransferoutResponse> TransferoutAsync(TransferoutRequest source);

        Task<QueryorderResponse> QueryorderAsync(QueryorderRequest source);

        Task<LaunchResponse> LaunchAsync(LaunchRequest source);

        Task<BetlistResponse> BetlistAsync(BetlistRequest source);

        Task<GamedetailResponse> GamedetailAsync(GamedetailRequest source);
        Task<GameListResponse> GameListAsync(GameListRequest source);


        Task<string> healthcheckAsync();
    }
}
