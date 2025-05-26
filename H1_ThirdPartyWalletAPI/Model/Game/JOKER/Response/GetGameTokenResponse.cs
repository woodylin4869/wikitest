namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;

public class GetGameTokenResponse
{
    /// <summary>
    /// 用于构建玩游戏 Url 的令牌（令牌只能使用一次）
    /// </summary>
    public string Token { get; set; }
    /// <summary>
    /// 玩家用户名
    /// </summary>
    public string Username { get; set; }
}