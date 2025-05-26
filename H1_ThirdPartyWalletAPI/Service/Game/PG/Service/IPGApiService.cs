using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.PG.Service
{
    /// <summary>
    /// PG API
    /// </summary>
    public interface IPGApiService
    {
        /// <summary>
        /// 创建玩家账号
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<CreateResponse> CreateAsync(CreateRequest source);

        /// <summary>
        /// 查询玩家的钱包余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetPlayerWalletResponse> GetPlayerWalletAsync(GetPlayerWalletRequest source);

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<TransferInResponse> TransferInAsync(TransferInRequest source);

        /// <summary>
        /// 转出余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<TransferOutResponse> TransferOutAsync(TransferOutRequest source);

        /// <summary>
        /// 转出所有余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<TransferAllOutResponse> TransferAllOutAsync(TransferAllOutRequest source);

        /// <summary>
        /// 取得遊戲連結
        /// </summary>
        /// <param name="gameCode"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        string GetGameUrl(string gameCode, GetGameUrlRequest source);

        /// <summary>
        /// 令牌验证
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        //Task<VerifySessionResponse> VerifySession(VerifySessionRequest source);

        /// <summary>
        /// 踢出玩家
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<KickResponse> KickAsync(KickRequest source);

        /// <summary>
        /// 冻结玩家
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<SuspendResponse> SuspendAsync(SuspendRequest source);

        /// <summary>
        /// 恢复玩家账号
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<ReinstateResponse> ReinstateAsync(ReinstateRequest source);

        /// <summary>
        /// 查看玩家状态
        /// 该 API 并非检查在线玩家的状态，而是检查在 PG 的状态。对于在线的活跃玩家，请在后台查询
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<CheckResponse> CheckAsync(CheckRequest source);

        /// <summary>
        /// 获取单个交易记录
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetSingleTransactionResponse> GetSingleTransactionAsync(GetSingleTransactionRequest source);

        /// <summary>
        /// 获取运营商令牌
        /// </summary>
        /// <returns></returns>
        Task<LoginProxyResponse> LoginProxyAsync();

        /// <summary>
        /// 投注详情页面
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<string> RedirectToBetDetail(RedirectToBetDetailRequest source);

        /// <summary>
        /// 获取多个玩家钱包余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetPlayersWalletResponse> GetPlayersWalletAsync(GetPlayersWalletRequest source);

        /// <summary>
        /// 获取历史记录
        /// 运营商可获得最近 60 天的投注历史记录。
        /// 运营商必须为每个请求提取至少 1500 条记录。
        /// 建議5分鐘調用一次API
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetHistoryResponse> GetHistoryAsync(GetHistoryRequest source);

        /// <summary>
        /// 获取特定时间内的历史记录
        /// 运营商可获得最近 60 天的投注历史记录。
        /// 运营商必须为每个请求提取至少 1500 条记录。
        /// 建議5分鐘調用一次API
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetHistoryForSpecificTimeRangeResponse> GetHistoryForSpecificTimeRangeAsync(GetHistoryForSpecificTimeRangeRequest source);

        /// <summary>
        /// 获取单一玩家的历史记录
        /// 运营商可获得最近 60 天的投注历史记录。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetPlayerHistoryResponse> GetPlayerHistoryAsync(GetPlayerHistoryRequest source);

        /// <summary>
        /// 获取游戏列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetGameListResponse> GetGameListAsync(GetGameListRequest source);

        /// <summary>
        /// 获取每小时投注汇总
        /// 运营商可以获取最近 60 天的投注记录
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetHandsSummaryHourlyResponse> GetHandsSummaryHourlyAsync(GetHandsSummaryHourlyRequest source);
        /// <summary>
        /// 获取在线玩家列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetOnlinePlayersResponse> GetOnlinePlayersAsync(GetOnlinePlayersRequest source);
    }
}
