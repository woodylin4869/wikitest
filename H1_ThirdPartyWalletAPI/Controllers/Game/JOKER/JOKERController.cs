using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.JOKER
{
    [Route("api/[controller]")]
    [ApiController]
    public class JOKERController : ControllerBase
    {
        private readonly IJokerApiService _jokerApiService;

        public JOKERController(IJokerApiService jokerApiService)
        {
            _jokerApiService = jokerApiService;
        }

        /// <summary>
        /// 获取游戏列表
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetGameList")]
        public async Task<GetGameListResponse> GetGameListAsync()
        {
            return await _jokerApiService.GetGameListAsync();
        }

        /// <summary>
        /// 取得遊戲 Token
        /// </summary>
        [HttpPost("GetGameToken")]
        public async Task<GetGameTokenResponse> GetGameTokenAsync(GetGameTokenRequest source)
        {
            return await _jokerApiService.GetGameTokenAsync(source);
        }

        /// <summary>
        /// 取得遊戲 Url
        /// </summary>
        [HttpPost("GetGameUrl")]
        public string GetGameUrl(GetGameUrlRequest source)
        {
            return _jokerApiService.GetGameUrl(source);
        }

        /// <summary>
        /// 获取信用
        /// </summary>
        [HttpPost("GetCredit")]
        public async Task<GetCreditResponse> GetCreditAsync(GetCreditRequest source)
        {
            return await _jokerApiService.GetCreditAsync(source);
        }

        /// <summary>
        /// 转移信用
        /// </summary>
        [HttpPost("TransferCredit")]
        public async Task<TransferCreditResponse> TransferCreditAsync(TransferCreditRequest source)
        {
            return await _jokerApiService.TransferCreditAsync(source);
        }

        /// <summary>
        /// 验证转移信用
        /// 响应 - 成功：HTTP / 1.1 200 OK
        /// 响应-失败：HTTP/1.1 404 Not Found 表示 requestId 不存在
        /// </summary>
        [HttpPost("ValidTransferCredit")]
        public async Task<ValidTransferCreditResponse> ValidTransferCreditAsync(ValidTransferCreditRequest source)
        {
            return await _jokerApiService.ValidTransferCreditAsync(source);
        }

        /// <summary>
        /// 提款所有信用
        /// </summary>
        [HttpPost("TransferOutAllCredit")]
        public async Task<TransferOutAllCreditResponse> TransferOutAllCreditAsync(TransferOutAllCreditRequest source)
        {
            return await _jokerApiService.TransferOutAllCreditAsync(source);
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        [HttpPost("CreatePlayer")]
        public async Task<CreatePlayerResponse> CreatePlayerAsync(CreatePlayerRequest source)
        {
            return await _jokerApiService.CreatePlayerAsync(source);
        }

        /// <summary>
        /// 注销用户
        /// </summary>
        [HttpPost("KickPlayer")]
        public async Task<KickPlayerResponse> KickPlayerAsync(KickPlayerRequest source)
        {
            return await _jokerApiService.KickPlayerAsync(source);
        }

        /// <summary>
        /// 取得注單明細
        /// </summary>
        [HttpPost("GetBetDetail")]
        public async Task<GetBetDetailResponse> GetBetDetailAsync(GetBetDetailRequest source)
        {
            return await _jokerApiService.GetBetDetailAsync(source);
        }

        /// <summary>
        /// 检索历史 URL
        /// </summary>
        [HttpPost("GetGameHistoryUrl")]
        public async Task<GetGameHistoryUrlResponse> GetGameHistoryUrlAsync(GetGameHistoryUrlRequest source)
        {
            return await _jokerApiService.GetGameHistoryUrlAsync(source);
        }

        /// <summary>
        /// 检索输赢
        /// </summary>
        [HttpPost("GetWinLoseSummary")]
        public async Task<GetWinLoseSummaryResponse> GetWinLoseSummaryAsync(GetWinLoseSummaryRequest source)
        {
            return await _jokerApiService.GetWinLoseSummaryAsync(source);
        }
    }
}
