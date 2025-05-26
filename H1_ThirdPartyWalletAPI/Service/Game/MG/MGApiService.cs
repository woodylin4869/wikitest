using H1_ThirdPartyWalletAPI.Controllers.Game.JDB;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using static Google.Rpc.Context.AttributeContext.Types;

namespace H1_ThirdPartyWalletAPI.Service.Game.MG
{
    public class MGApiService : MGApiServiceBase, IMGApiService
    {
        public MGApiService(ILogger<MGApiService> logger, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, IApiHealthCheckService apiHealthCheckService) : base( logger, httpClientFactory, memoryCache, apiHealthCheckService)
        {

        }
        #region Players
        /// <summary>
        /// Get content URL 获取内容网址
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetGameUrlResponse> GetGameUrl(GetGameUrlRequest request)
        {
            var response = await PostAsync<GetGameUrlRequest, GetGameUrlResponse>(request, $"players/{request.PlayerId}/sessions");
            return response;
        }

        /// <summary>
        /// Get player details 获取玩家信息 (取得balance)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetBalanceResponse> GetBalance(GetBalanceRequest request)
        {
            var response = await GetAsync<GetBalanceResponse>($"players/{request.PlayerId}?properties=balance");
            return response;
        }

        /// <summary>
        /// Create player 创建玩家
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CreatePlayerResponse> CreatePlayer(CreatePlayerRequest request)
        {
            var response = await PostAsync<CreatePlayerRequest, CreatePlayerResponse>(request, $"players");
            return response;
        }

        [Obsolete("Method is obsolete :( 🈲 MG 目前沒使用到)")]
        public async Task<CreatePlayerResponse> UpdatePlayer(UpdatePlayerRequest request)
        {
            var response = await PatchAsync<UpdatePlayerRequest, CreatePlayerResponse>(request, $"Players/{request.PlayerId}");
            return response;
        }
        #endregion Players

        /// <summary>
        /// Get game details list 获取游戏详情列表
        /// </summary>
        /// <returns></returns>
        public async Task<GetGameListResponse> GetGameList()
        {
            var result = await GetAsync<List<ProductInfo>>($"games");
            return new GetGameListResponse { Data = result };

        }

        [Obsolete("Method is obsolete :( 🈲 MG 目前沒使用到)")]
        public async Task<ProductInfo> GetGame(string gameCode)
        {
            var result = await GetAsync<ProductInfo>($"games/{gameCode}");
            return result;
        }

        #region BetRecord

        /// <summary>
        /// Get bets details (bet by bet) 获取下注信息
        /// 注单- 10天的期限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetBetRecordHistoryResponse> GetBetRecordHistory(GetBetRecordHistoryRequest request)
        {
            var queryString = typeof(GetBetRecordHistoryRequest).GetProperties().Where(x => x.GetValue(request) != null).Select(x => string.Format("{0}={1}", x.Name, x.GetValue(request))).ToArray();
            string path = string.Format("bets?{0}", string.Join('&', queryString));
            var betRecords = await GetAsync<List<BetRecord>>(path);
            GetBetRecordHistoryResponse response = new() { BetRecords = betRecords };
            return response;
        }

        /// <summary>
        /// 返回玩家的未结束下注
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetIncompleteBetsResponse> GetIncompleteBets(GetIncompleteBetsRequest request)
        {
            string path = $"players/{request.PlayerId}/incompleteBets";
            var result = await GetAsync<List<ProductIncompleteBet>>(path);
            return new()
            {
                IncompleteBet = result
            };
        }
        #endregion BetRecord

        #region Transaction

        /// <summary>
        /// Create transaction 创建资金交易
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="200">交易信息已经获取</response>
        /// <response code="201">交易创建成功</response>
        /// <response code="400">请求无效 - 输入验证失败</response>
        /// <response code="401">未经授权</response>
        /// <response code="409">由于冲突无法处理请求，比如没有足够的资金</response>
        /// <response code="500">内部服务器错误</response>
        public async Task<PostTransactionResponse> PostTransaction(PostTransactionRequest request)
        {
            var response = await PostAsync<PostTransactionRequest, PostTransactionResponse>(request, $"WalletTransactions");
            return response;
        }

        /// <summary>
        /// Get transaction details by idempotencyKey 通过幂等键获取交易详细信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="200">交易信息已经获取</response>
        /// <response code="400">交易不存在</response>
        /// <response code="401">未经授权</response>
        /// <response code="500">内部服务器错误</response>
        public async Task<GetTransactionResponse> GetTransaction(GetTransactionRequest request)
        {
            var response = await GetAsyncMsgIdHttpStatusCode<GetTransactionResponse>($"WalletTransactions?idempotencyKey={request.idempotencyKey}");
            return response;
        }

        /// <summary>
        /// Get transaction details 获取交易信息
        /// </summary>
        /// <param name="TransactionId"></param>
        /// <returns></returns>
        [Obsolete("Method is obsolete :( 🈲 MG 目前沒使用到)")]
        public async Task<GetTransactionResponse> GetTransactionByTransactionId(string TransactionId)
        {
            var response = await GetAsync<GetTransactionResponse>($"WalletTransactions/{TransactionId}");
            return response;
        }
        #endregion Transaction

        #region Reports
        /// <summary>
        /// Get detailed financial report 获取详细资金报表
        /// 小时 - 30天的期限
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetFinacialResponse> GetFinancial(GetFinacialRequest request)
        {
            var response = await PostAsyncJsonBody<GetFinacialRequest, List<FinacialReport>>(request, $"reports/financial");
            return new GetFinacialResponse { data = response };
        }
        #endregion Reports

        /// <summary>
        /// 遊戲商開牌紀錄
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GameDetailUrlResponse> GameDetailURL(GameDetailUrlRequest request)
        {
            List<GameDetailUrlResponse> response = await PostAsync<GameDetailUrlRequest, List<GameDetailUrlResponse>>(request, $"players/{request.playerId}/betVisualizers");
            return response[0];
        }

        /// <summary>
        /// 活動派彩
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<List<tournamentWinsResponse>> TournamentWins(TournamentWinsRequest source)
        {
            return await PostAsyncJsonBody<TournamentWinsRequest, List<tournamentWinsResponse>>(source, $"reports/tournamentWins");
        }
        /// <summary>
        /// 新活動派彩
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<List<FortuneRewardsResponse>> FortuneRewards(FortuneRewardsRequest source)
        {
            return await PostAsyncJsonBody<FortuneRewardsRequest, List<FortuneRewardsResponse>>(source, $"reports/fortuneRewards");
        }
        /// <summary>
        /// 实时系统监测
        /// </summary>
        /// <returns></returns>
        public async Task HeartBeat()
        {
            var result = await GetHeartBeatAsync();
        }
    }
}
