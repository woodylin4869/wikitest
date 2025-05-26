using H1_ThirdPartyWalletAPI.Model.Game.PG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Response;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Game.PG.Service;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.PG
{
    /// <summary>
    /// PG API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class PGController : ControllerBase
    {
        private readonly IPGApiService _pgApiService;
        private readonly IDBService _dbIdbService;
        private readonly ITransferWalletService _transferWalletService;

        public PGController(IPGApiService pgApiService, IDBService dbIdbService, ITransferWalletService transferWalletService)
        {
            _pgApiService = pgApiService;
            _dbIdbService = dbIdbService;
            _transferWalletService = transferWalletService;
        }

        /// <summary>
        /// 创建玩家账号
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Create")]
        public async Task<CreateResponse> CreateAsync([FromBody] CreateRequest source)
        {
            return await _pgApiService.CreateAsync(source);
        }

        /// <summary>
        /// 查询玩家的钱包余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetPlayerWallet")]
        public async Task<GetPlayerWalletResponse> GetPlayerWalletAsync([FromBody] GetPlayerWalletRequest source)
        {
            return await _pgApiService.GetPlayerWalletAsync(source);
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TransferIn")]
        public async Task<TransferInResponse> TransferInAsync([FromBody] TransferInRequest source)
        {
            return await _pgApiService.TransferInAsync(source);
        }

        /// <summary>
        /// 转出余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TransferOut")]
        public async Task<TransferOutResponse> TransferOutAsync([FromBody] TransferOutRequest source)
        {
            return await _pgApiService.TransferOutAsync(source);
        }

        /// <summary>
        /// 转出所有余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("TransferAllOut")]
        public async Task<TransferAllOutResponse> TransferAllOutAsync([FromBody] TransferAllOutRequest source)
        {
            return await _pgApiService.TransferAllOutAsync(source);
        }

        /// <summary>
        /// 取得遊戲連結
        /// operator_player_session 請帶 pg_id
        /// </summary>
        /// <param name="gameCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetGameUrl/{gameCode}")]
        public string GetGameUrl(string gameCode, [FromBody] GetGameUrlRequest source)
        {
            return _pgApiService.GetGameUrl(gameCode, source);
        }

        /// <summary>
        /// 令牌验证
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("VerifySession")]
        public async Task<VerifySessionResponse> VerifySession([FromForm] VerifySessionRequest source)
        {
            return await VerifySessionService(source);
        }

        /// <summary>
        /// 踢出玩家
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Kick")]
        public async Task<KickResponse> KickAsync([FromBody] KickRequest source)
        {
            return await _pgApiService.KickAsync(source);
        }

        /// <summary>
        /// 冻结玩家
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Suspend")]
        public async Task<SuspendResponse> SuspendAsync([FromBody] SuspendRequest source)
        {
            return await _pgApiService.SuspendAsync(source);
        }

        /// <summary>
        /// 恢复玩家账号
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Reinstate")]
        public async Task<ReinstateResponse> ReinstateAsync([FromBody] ReinstateRequest source)
        {
            return await _pgApiService.ReinstateAsync(source);
        }

        /// <summary>
        /// 查看玩家状态
        /// 该 API 并非检查在线玩家的状态，而是检查在 PG 的状态。对于在线的活跃玩家，请在后台查询
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Check")]
        public async Task<CheckResponse> CheckAsync([FromBody] CheckRequest source)
        {
            return await _pgApiService.CheckAsync(source);
        }

        /// <summary>
        /// 获取单个交易记录
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetSingleTransaction")]
        public async Task<GetSingleTransactionResponse> GetSingleTransactionAsync([FromBody] GetSingleTransactionRequest source)
        {
            return await _pgApiService.GetSingleTransactionAsync(source);
        }

        /// <summary>
        /// 投注详情页面
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("RedirectToBetDetail")]
        public async Task<string>  RedirectToBetDetail([FromBody] RedirectToBetDetailRequest source)
        {
            return await _pgApiService.RedirectToBetDetail(source);
        }

        /// <summary>
        /// 获取多个玩家钱包余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetPlayersWallet")]
        public async Task<GetPlayersWalletResponse> GetPlayersWalletAsync([FromBody] GetPlayersWalletRequest source)
        {
            return await _pgApiService.GetPlayersWalletAsync(source);
        }

        /// <summary>
        /// 获取历史记录
        /// 运营商可获得最近 60 天的投注历史记录。
        /// 运营商必须为每个请求提取至少 1500 条记录。
        /// 建議5分鐘調用一次API
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetHistory")]
        public async Task<GetHistoryResponse> GetHistoryAsync([FromBody] GetHistoryRequest source)
        {
            return await _pgApiService.GetHistoryAsync(source);
        }

        /// <summary>
        /// 获取特定时间内的历史记录
        /// 运营商可获得最近 60 天的投注历史记录。
        /// 运营商必须为每个请求提取至少 1500 条记录。
        /// 建議5分鐘調用一次API
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetHistoryForSpecificTimeRange")]
        public async Task<GetHistoryForSpecificTimeRangeResponse> GetHistoryForSpecificTimeRangeAsync([FromBody] GetHistoryForSpecificTimeRangeRequest source)
        {
            return await _pgApiService.GetHistoryForSpecificTimeRangeAsync(source);
        }

        /// <summary>
        /// 获取单一玩家的历史记录
        /// 运营商可获得最近 60 天的投注历史记录。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetPlayerHistory")]
        public async Task<GetPlayerHistoryResponse> GetPlayerHistoryAsync([FromBody] GetPlayerHistoryRequest source)
        {
            return await _pgApiService.GetPlayerHistoryAsync(source);
        }

        private async Task<VerifySessionResponse> VerifySessionService(VerifySessionRequest source)
        {
            var outputModel = new VerifySessionResponse();

            if (source.operator_token != Config.CompanyToken.PG_Token)
            {
                outputModel.error = new VerifySessionResponse.Error
                {
                    code = "1034"
                };
                return outputModel;
            }

            if (source.secret_key != Config.CompanyToken.PG_Key)
            {
                outputModel.error = new VerifySessionResponse.Error
                {
                    code = "1034"
                };
                return outputModel;
            }


            var target = await _dbIdbService.GetGamePlatformPGUser(source.operator_player_session); // operator_player_session 為 pg_id

            // 驗證發出去的 PGId
            if (target == null)
            {
                outputModel.error = new VerifySessionResponse.Error
                {
                    code = "1034"
                };
                return outputModel;
            }

            var wallet = await _transferWalletService.GetWalletCache(target.club_id);

            return new VerifySessionResponse
            {
                data = new VerifySessionResponse.Data
                {
                    player_name = source.operator_player_session,
                    nickname = wallet.Club_Ename,
                    currency = wallet.Currency
                }
            };
        }
    }
}
