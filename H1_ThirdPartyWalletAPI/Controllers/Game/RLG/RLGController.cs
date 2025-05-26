using H1_ThirdPartyWalletAPI.Model.Game.RLG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RLG.Response;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.RLG;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.RLG
{
    /// <summary>
    /// RLG API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class RLGController : ControllerBase
    {

        private readonly IDBService _dbIdbService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly IRLGApiService _irlgapiservice;
        public RLGController(IDBService dbIdbService, ITransferWalletService transferWalletService, IRLGApiService irlgapiservice)
        {
            _dbIdbService = dbIdbService;
            _transferWalletService = transferWalletService;
            _irlgapiservice = irlgapiservice;
        }
        /// <summary>
        /// 取得 URL Token
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetURLToken")]
        public async Task<GetURLTokenResponse> GetURLTokenAsync([FromBody] GetURLTokenRequest source)
        {
            return await _irlgapiservice.GetURLTokenAsync(source);
        }

        /// <summary>
        /// 建立與更新會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Route("CreateOrSetUser")]
        [HttpPost]
        public async Task<CreateOrSetUserResponse> CreateOrSetUserAsync([FromBody] CreateOrSetUserRequest source)
        {
            return await _irlgapiservice.CreateOrSetUserAsync(source);
        }

        /// <summary>
        /// 注單資訊
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Route("BetInfo")]
        [HttpPost]
        public async Task<IActionResult> BetInfoAsync([FromBody] BetInfoRequest source)
        {
            switch (source.SetOption)//switch (比對的運算式)
            {
                case 0:
                    return Ok(await _irlgapiservice.BetInfoJsonnAsync(source));
                case 2:
                    return Ok(await _irlgapiservice.BetInfourlAsync(source));
                default:
                    return BadRequest();
            }
        }
        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Route("Deposit")]
        [HttpPost]
        public async Task<DepositResponse> DepositAsync([FromBody] DepositRequest source)
        {
            return await _irlgapiservice.DepositAsync(source);

        }
        /// <summary>
        /// 查詢玩家在線列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Route("PlayerOnlineList")]
        [HttpPost]
        public async Task<PlayerOnlineListResponse> PlayerOnlineListAsync([FromBody] PlayerOnlineListRequest source)
        {
            return await _irlgapiservice.PlayerOnlineListAsync(source);
        }
        /// <summary>
        /// 剔除玩家
        /// </summary>
        [Route("Kickout")]
        [HttpPost]
        public async Task<KickoutResponse> KickoutAsync([FromBody] KickoutRequest source)
        {
            return await _irlgapiservice.KickoutAsync(source);
        }
        /// <summary>
        /// 會員點數交易分頁列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Route("TransferRecord")]
        [HttpPost]
        public async Task<TransferRecordResponse> TransferRecordAsync([FromBody] TransferRecordRequest source)
        {
            return await _irlgapiservice.TransferRecordAsync(source);
        }

        /// <summary>
        /// 會員投注紀錄分頁列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Route("GetBetRecord")]
        [HttpPost]
        public async Task<GetBetRecordResponse> GetBetRecordAsync([FromBody] GetBetRecordRequest source)
        {
            return await _irlgapiservice.GetBetRecordAsync(source);
        }

        /// <summary>
        /// 批次查詢餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Route("BatchBalance")]
        [HttpPost]
        public async Task<BatchBalanceResponse> BatchBalanceAsync([FromBody] BatchBalanceRequest source)
        {
            var data = new BatchBalancepostdata
            {
                SystemCode = source.SystemCode,
                Data = JsonConvert.SerializeObject(source.Data)
            };

            return await _irlgapiservice.BatchBalanceAsync(data);
        }
        /// <summary>
        /// 批次會員提款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Route("BatchWithdrawal")]
        [HttpPost]
        public async Task<BatchWithdrawalResponse> BatchWithdrawalAsync([FromBody] BatchWithdrawalRequest source)
        {

            var data = new BatchWithdrawalpostdata
            {
                SystemCode = source.SystemCode,
                Data = JsonConvert.SerializeObject(source.Data)
            };

            return await _irlgapiservice.BatchWithdrawalAsync(data);
        }
        /// <summary>
        /// 會員投注總計列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Route("GetBetTotalList")]
        [HttpPost]
        public async Task<GetBetTotalListResponse> GetBetTotalListAsync([FromBody] GetBetTotalListRequest source)
        {
            var serverdata = new GetBetTotalListserverRequest()
            {
                SystemCode = source.SystemCode,
                WebId = source.WebId,
                GameId = source.GameId,
                StartTime = source.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                EndTime = source.EndTime.ToString("yyyy-MM-dd HH:mm:ss")
            };

            return await _irlgapiservice.GetBetTotalListAsync(serverdata);
        }

        /// <summary>
        /// 目前開放遊戲
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [Route("GetOpenGame")]
        [HttpPost]
        public async Task<GetOpenGameResponse> GetOpenGameAsync([FromBody] GetOpenGameRequest source)
        {
            return await _irlgapiservice.GetOpenGameAsync(source);
        }
    }
}
