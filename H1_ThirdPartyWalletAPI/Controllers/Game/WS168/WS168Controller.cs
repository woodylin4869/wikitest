using H1_ThirdPartyWalletAPI.Model.Game.WS168.Request;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Response;
using H1_ThirdPartyWalletAPI.Service.Game.WS168;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.WS168
{
    [Route("/[controller]")]
    [ApiController]
    public class WS168Controller : ControllerBase
    {
        private readonly IWS168ApiService _ims168ApiService;

        public WS168Controller(IWS168ApiService ims168ApiService)
        {
            _ims168ApiService = ims168ApiService;
        }
        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreatePlayer")]
        public async Task<CreatePlayerResponse> CreatePlayerAsync([FromBody] CreatePlayerRequest source)
        {
            return await _ims168ApiService.CreatePlayerAsync(source);
        }
        /// <summary>
        /// 進入遊戲URL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PlayerLogin")]
        public async Task<PlayerLoginResponse> PlayerLoginAsync([FromBody] PlayerLoginRequest source)
        {
            return await _ims168ApiService.PlayerLoginAsync(source);
        }
        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Deposit")]
        public async Task<DepositResponse> DepositAsync([FromBody] DepositRequest source)
        {
            return await _ims168ApiService.DepositAsync(source);
        }
        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Withdraw")]
        public async Task<WithdrawResponse> WithdrawAsync([FromBody] WithdrawRequest source)
        {
            return await _ims168ApiService.WithdrawAsync(source);
        }
        /// <summary>
        /// 查詢餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryPlayerBalance")]
        public async Task<QueryPlayerBalanceResponse> QueryPlayerBalanceAsync([FromBody] QueryPlayerBalanceRequest source)
        {
            return await _ims168ApiService.QueryPlayerBalanceAsync(source);
        }
        /// <summary>
        /// 查詢交易狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SearchingOrdersStatus")]
        public async Task<SearchingOrdersStatusResponse> SearchingOrdersStatusAsync([FromBody] SearchingOrdersStatusRequest source)
        {
            return await _ims168ApiService.SearchingOrdersStatusAsync(source);
        }
        /// <summary>
        /// 取得住單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("BetLog")]
        public async Task<SearchingOrdersStatusResponse> BetLogAsync([FromBody] BetLogRequest source)
        {
            return await _ims168ApiService.BetLogAsync(source);
        }
    }
}
