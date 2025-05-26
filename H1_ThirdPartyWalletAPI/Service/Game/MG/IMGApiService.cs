using H1_ThirdPartyWalletAPI.Model.Game.MG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Response;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.MG
{
    public interface IMGApiService
    {
        /// <summary>
        /// Create player 创建玩家
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<CreatePlayerResponse> CreatePlayer(CreatePlayerRequest request);

        /// <summary>
        /// Get player details 获取玩家信息 (取得balance)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GetBalanceResponse> GetBalance(GetBalanceRequest request);
        /// <summary>
        /// Get bets details (bet by bet) 获取下注信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GetBetRecordHistoryResponse> GetBetRecordHistory(GetBetRecordHistoryRequest request);
        /// <summary>
        /// Get detailed financial report 获取详细资金报表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GetFinacialResponse> GetFinancial(GetFinacialRequest request);
        /// <summary>
        /// 🈲 MG 目前沒使用到
        /// </summary>
        /// <param name="gameCode"></param>
        /// <returns></returns>
        Task<ProductInfo> GetGame(string gameCode);
        /// <summary>
        /// Get game details list 获取游戏详情列表
        /// </summary>
        /// <returns></returns>
        Task<GetGameListResponse> GetGameList();
        /// <summary>
        /// Get content URL 获取内容网址
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GetGameUrlResponse> GetGameUrl(GetGameUrlRequest request);
        /// <summary>
        /// Get transaction details by idempotencyKey 通过幂等键获取交易详细信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <response code="200">交易信息已经获取</response>
        /// <response code="400">交易不存在</response>
        /// <response code="401">未经授权</response>
        /// <response code="500">内部服务器错误</response>
        Task<GetTransactionResponse> GetTransaction(GetTransactionRequest request);
        /// <summary>
        /// Get transaction details 获取交易信息
        /// 🈲 MG 目前沒使用到
        /// </summary>
        /// <param name="TransactionId"></param>
        /// <returns></returns>
        Task<GetTransactionResponse> GetTransactionByTransactionId(string TransactionId);
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
        Task<PostTransactionResponse> PostTransaction(PostTransactionRequest request);
        /// <summary>
        /// 🈲 MG 目前沒使用到
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<CreatePlayerResponse> UpdatePlayer(UpdatePlayerRequest request);
        /// <summary>
        /// 遊戲商開牌紀錄
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GameDetailUrlResponse> GameDetailURL(GameDetailUrlRequest request);
        Task<GetIncompleteBetsResponse> GetIncompleteBets(GetIncompleteBetsRequest request);

        /// <summary>
        /// 取得活動派彩
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<List<tournamentWinsResponse>> TournamentWins(TournamentWinsRequest source);
        /// <summary>
        /// 新的 取得活動派彩
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<List<FortuneRewardsResponse>> FortuneRewards(FortuneRewardsRequest source);
        Task HeartBeat();
    }
}
