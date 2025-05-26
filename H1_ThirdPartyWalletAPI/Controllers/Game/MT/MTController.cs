using H1_ThirdPartyWalletAPI.Model.Game.MT.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MT.Response;
using H1_ThirdPartyWalletAPI.Service.Game.MT;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace H1_ThirdPartyWalletAPI.Controllers.Game.MT
{
    [Route("/[controller]")]
    [ApiController]
    public class MTController : ControllerBase
    {
        private readonly IMTApiService _imsmtApiService;

        public MTController(IMTApiService imsmtApiService)
        {
            _imsmtApiService = imsmtApiService;
        }
        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("playerCreate")]
        public async Task<playerCreate2Response> playerCreateAsync([FromBody] PlayerCreateRequest source)
        {
            return await _imsmtApiService.playerCreateAsync(source);
        }
        /// <summary>
        /// 查詢餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getPlayerBalance")]
        public async Task<getPlayerBalanceResponse> getPlayerBalanceAsync([FromBody] getPlayerBalanceRequest source)
        {
            return await _imsmtApiService.getPlayerBalanceAsync(source);
        }

        /// <summary>
        /// 轉入
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("deposit2")]
        public async Task<deposit2Response> deposit2Async([FromBody] deposit2Request source)
        {
            return await _imsmtApiService.deposit2Async(source);
        }
        /// <summary>
        /// 轉出
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("withdraw2")]
        public async Task<withdraw2Response> withdraw2Async([FromBody] withdraw2Request source)
        {
            return await _imsmtApiService.withdraw2Async(source);
        }
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("playerPlatformUrl")]
        public async Task<playerPlatformUrlResponse> playerPlatformUrlAsync([FromBody] PlayerPlatformUrlRequest source)
        {
            return await _imsmtApiService.playerPlatformUrlAsync(source,new PlayerPlatformUrlrawData() {gameHall="2",lang="ZH-TW" });
        }
        /// <summary>
        /// 查詢交易狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("queryTransbyId")]
        public async Task<QueryTransbyIdResponse> queryTransbyIdAsync([FromBody] QueryTransbyIdRequest source)
        {
            return await _imsmtApiService.queryTransbyIdAsync(source);
        }
        /// <summary>
        /// 取得住單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("queryMerchantGameRecord2")]
        public async Task<queryMerchantGameRecord2Response> queryMerchantGameRecord2Async([FromBody] QueryMerchantGameRecord2rawData source)
        {
            return await _imsmtApiService.queryMerchantGameRecord2Async(source);
        }

        [HttpPost]
        [Route("logOutGame")]
        public async Task<logOutGameResponse> logOutGameAsync([FromBody] logOutGameRequest source)
        {
            return await _imsmtApiService.logOutGameAsync(source);
        }
    }
}
