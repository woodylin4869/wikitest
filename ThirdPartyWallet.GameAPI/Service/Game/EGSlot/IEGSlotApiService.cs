using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.EGSlot.Request;
using ThirdPartyWallet.Share.Model.Game.EGSlot.Response;


namespace H1_ThirdPartyWalletAPI.Service.Game.EGSlot
{
    public interface IEGSlotApiService
    {
        Task<PlayersResponse> PlayersAsync(PlayersRequest source);
        Task<StatusResponse> StatusAsync(StatusRequest source);
        Task<TransferinResponse> TransferinAsync(TransferinRequest source);
        Task<TransferoutResponse> TransferoutAsync(TransferoutRequest source);
        Task<LoginResponse> LoginAsync(LoginRequest source);
        Task<LogoutResponse> LogoutAsync(LogoutRequest source);
        Task<LogoutAllResponse> LogoutAllAsync(LogoutAllRequest source);
        Task<TransactionResponse> TransactionAsync(TransactionRequest source);
        Task<TransferHistoryResponse> TransferHistoryAsync(TransferHistoryRequest source);

        Task<GetdetailurlResponse> GetdetailurlAsync(GetdetailurlRequest source);

        Task<GethourdataResponse> GethourdataAsync(GethourdataRequest source);

        Task<GetagentsResponse> GetagentsAsync();
    }
}
