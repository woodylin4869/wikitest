using H1_ThirdPartyWalletAPI.Model.Game.MG.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Response;
using H1_ThirdPartyWalletAPI.Service.Game.MG;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.MG
{
    [Route("MG/api/")]
    [ApiController]
    public class MGController : ControllerBase
    {
        private readonly IMGApiService mGApiService;

        public MGController(IMGApiService mGApiService)
        {
            this.mGApiService = mGApiService;
        }

        /// <summary>
        /// Get game details list 获取游戏详情列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetGames")]
        [Produces("application/json")]
        public async Task<GetGameListResponse> GetGames()
        {
            var response = await mGApiService.GetGameList();
            return response;
        }
        /// <summary>
        /// GameGameUrl
        /// Get content URL 获取内容网址
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /agents/{agentCode}/players/{playerId}/sessions
        ///     {
        ///         "playerId": "DEV2200011078", 
        ///         "langCode": "en", 
        ///         "contentCode": "SMG_108Heroes", 
        ///         "contentType": "Desktop"
        ///     }
        /// </remarks>
        /// <param name="playerId"></param>
        /// <param name="langCode"></param>
        /// <param name="gameCode"></param>
        /// <param name="platform">Unknown,Desktop, Mobile</param>
        /// <param name="homeUrl"></param>
        /// <returns></returns>
        /// <response code="201">内容网址返回成功</response>
        /// <response code="400">请求无效 - 输入验证失败</response>
        /// <response code="401">未经授权</response>
        /// <response code="409">由于冲突无法处理请求，比如为锁定的玩家启动游戏链接</response>
        /// <response code="500">内部服务器错误</response>
        [HttpPost("GetGameUrl")]
        [Produces("application/json")]
        public async Task<string> GetGameUrl(string playerId, string langCode, string gameCode, string platform, string homeUrl)
        {
            var request = new GetGameUrlRequest { PlayerId = playerId, langCode = langCode, platform = (Model.Game.MG.Enum.Platform)Enum.Parse(typeof(Model.Game.MG.Enum.Platform), platform), contentCode = gameCode, homeUrl = homeUrl };
            var result = await mGApiService.GetGameUrl(request);
            return result.GameUrl;
        }
        /// <summary>
        /// CreateUser
        /// Create player 创建玩家
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /agents/{agentCode}/players
        ///     {
        ///         "playerId": "DEV2200011078" 
        ///     }
        /// </remarks>
        /// <param name="playerId"></param>
        /// <returns>CreatePlayerResponse</returns>
        /// <response code="200">玩家已经存在。存在的玩家已经获取</response>
        /// <response code="201">成功创建玩家</response>
        /// <response code="400">请求无效 - 输入验证失败</response>
        /// <response code="401">未经授权</response>
        /// <response code="500">内部服务器错误</response>
        [HttpPost("CreateUser")]
        [Produces("application/json")]
        public async Task<CreatePlayerResponse> CreateUser(string playerId)
        {
            var request = new CreatePlayerRequest { PlayerId = playerId };
            var result = await mGApiService.CreatePlayer(request);
            return result;
        }
        /// <summary>
        /// GetUserBalance
        /// Get player details 获取玩家信息 (取得balance)
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /agents/{{agentCode}}/players/:playerId?properties=balance
        ///     {
        ///     }
        /// </remarks>
        /// <param name="playerId">playerId</param>
        /// <returns>GetBalanceResponse</returns>
        /// <response code="200">玩家信息已经获取</response>
        /// <response code="400">请求无效 - 输入验证失败</response>
        /// <response code="401">未经授权</response>
        /// <response code="404">没有找到玩家</response>
        /// <response code="500">内部服务器错误</response>
        [HttpGet("GetUserBalance")]
        [Produces("application/json")]
        public async Task<GetBalanceResponse> GetUserBalnace(string playerId) 
        {
            var request = new GetBalanceRequest { PlayerId = playerId };
            var result = await mGApiService.GetBalance(request);
            return result;

        }
        /// <summary>
        /// Deposit (存款)
        /// Create transaction 创建资金交易 Deposit (存款)
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="playerId"></param>
        /// <param name="amount"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /agents/{agentCode}/WalletTransactions
        ///     {
        ///         "transactionId": "446C638C-37AF-419A-8469-C4F3D0A8FDF8",
        ///         "playerId": "DEV2200011078",
        ///         "amount": 100
        ///     }
        /// </remarks>
        /// <returns></returns>
        /// <response code="200">交易信息已经获取</response>
        /// <response code="201">交易创建成功</response>
        /// <response code="400">请求无效 - 输入验证失败</response>
        /// <response code="401">未经授权</response>
        /// <response code="409">由于冲突无法处理请求，比如没有足够的资金</response>
        /// <response code="500">内部服务器错误</response>
        [HttpPost("Deposit")]
        [Produces("application/json")]
        public async Task<PostTransactionResponse> Depoit(Guid transactionId, string playerId, decimal amount)
        {
            var request = new PostTransactionRequest { PlayerId = playerId, Amount = amount, ExternalTransactionId = transactionId.ToString(), IdempotencyKey = transactionId.ToString(), Type = TransactionType.Deposit };
            var result = await mGApiService.PostTransaction(request);
            return result;
        }
        /// <summary>
        /// Withdraw (提款)
        /// Create transaction 创建资金交易 Withdraw (提款)
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="playerId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /agents/{agentCode}/WalletTransactions
        ///     {
        ///         "transactionId": "446C638C-37AF-419A-8469-C4F3D0A8FDF8",
        ///         "playerId": "DEV2200011078",
        ///         "amount": 100
        ///     }
        /// </remarks>
        /// <response code="200">交易信息已经获取</response>
        /// <response code="201">交易创建成功</response>
        /// <response code="400">请求无效 - 输入验证失败</response>
        /// <response code="401">未经授权</response>
        /// <response code="409">由于冲突无法处理请求，比如没有足够的资金</response>
        /// <response code="500">内部服务器错误</response>
        [HttpPost("Withdraw")]
        [Produces("application/json")]
        public async Task<PostTransactionResponse> Withdraw(Guid transactionId, string playerId, decimal amount)
        {
            var request = new PostTransactionRequest { PlayerId = playerId, Amount = amount, ExternalTransactionId = transactionId.ToString(), IdempotencyKey = transactionId.ToString(), Type = TransactionType.Withdraw };
            var result = await mGApiService.PostTransaction(request);
            return result;
        }

        /// <summary>
        /// 取得活動派彩
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="utcOffset"></param>
        /// <param name="tournaments"></param>
        /// <returns></returns>
        [HttpPost("TournamentWins")]
        public async Task<List<tournamentWinsResponse>> TournamentWins(DateTime fromDate,DateTime toDate,int utcOffset,int[] tournaments)
        {
            var source = new TournamentWinsRequest()
            {
                fromDate= fromDate,
                toDate= toDate,
                utcOffset= utcOffset,
                tournaments= tournaments
            };
            return await mGApiService.TournamentWins(source);
        }

        [HttpPost("HeartBeat")]
        public async Task HeartBeat()
        {
            await mGApiService.HeartBeat();
        }
    }
}

