using H1_ThirdPartyWalletAPI.Model.Game.MT.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MT.Response;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.MT
{
    public interface IMTApiService
    {
        Task<playerCreate2Response> playerCreateAsync(PlayerCreateRequest source);
        Task<getPlayerBalanceResponse> getPlayerBalanceAsync(getPlayerBalanceRequest source);

        Task<deposit2Response> deposit2Async(deposit2Request source);

        Task<withdraw2Response> withdraw2Async(withdraw2Request source);

        Task<playerPlatformUrlResponse> playerPlatformUrlAsync(PlayerPlatformUrlRequest source, PlayerPlatformUrlrawData rawData);

        Task<QueryTransbyIdResponse> queryTransbyIdAsync(QueryTransbyIdRequest source);

        Task<queryMerchantGameRecord2Response> queryMerchantGameRecord2Async(QueryMerchantGameRecord2rawData source);

        Task<logOutGameResponse> logOutGameAsync( logOutGameRequest source);

        Task<playCheckUrlResponse> playCheckUrlAsync(playCheckUrlrawData rawData);

        Task<queryMerchantGameDataResponse> queryMerchantGameDataAsync(queryMerchantGameDatarawData rawData);
    }
}
