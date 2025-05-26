using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.JOKER;

public interface IJokerApiService
{
    /// <summary>
    /// 获取游戏列表
    /// </summary>
    /// <returns></returns>
    Task<GetGameListResponse> GetGameListAsync();

    /// <summary>
    /// 取得遊戲 Token
    /// </summary>
    Task<GetGameTokenResponse> GetGameTokenAsync(GetGameTokenRequest source);

    /// <summary>
    /// 取得遊戲 Url
    /// </summary>
    string GetGameUrl(GetGameUrlRequest source);

    /// <summary>
    /// 获取信用
    /// </summary>
    Task<GetCreditResponse> GetCreditAsync(GetCreditRequest source);

    /// <summary>
    /// 转移信用
    /// </summary>
    Task<TransferCreditResponse> TransferCreditAsync(TransferCreditRequest source);

    /// <summary>
    /// 验证转移信用
    /// 响应 - 成功：HTTP / 1.1 200 OK
    /// 响应-失败：HTTP/1.1 404 Not Found 表示 requestId 不存在
    /// </summary>
    Task<ValidTransferCreditResponse> ValidTransferCreditAsync(ValidTransferCreditRequest source);

    /// <summary>
    /// 提款所有信用
    /// </summary>
    Task<TransferOutAllCreditResponse> TransferOutAllCreditAsync(TransferOutAllCreditRequest source);

    /// <summary>
    /// 创建用户
    /// </summary>
    Task<CreatePlayerResponse> CreatePlayerAsync(CreatePlayerRequest source);

    /// <summary>
    /// 注销用户
    /// </summary>
    Task<KickPlayerResponse> KickPlayerAsync(KickPlayerRequest source);

    /// <summary>
    /// 取得注單明細
    /// </summary>
    Task<GetBetDetailResponse> GetBetDetailAsync(GetBetDetailRequest source);

    /// <summary>
    /// 检索历史 URL
    /// </summary>
    Task<GetGameHistoryUrlResponse> GetGameHistoryUrlAsync(GetGameHistoryUrlRequest source);

    /// <summary>
    /// 检索输赢
    /// </summary>
    Task<GetWinLoseSummaryResponse> GetWinLoseSummaryAsync(GetWinLoseSummaryRequest source);
    /// <summary>
    /// 小時帳
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    Task<GethourBetResponse> GethourBetAsync(GethourBetRequest source);
}