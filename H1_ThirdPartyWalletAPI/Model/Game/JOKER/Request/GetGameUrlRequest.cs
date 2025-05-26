namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;

public class GetGameUrlRequest
{
    /// <summary>
    /// 遊戲 Token
    /// </summary>
    public string Token { get; set; }
    /// <summary>
    /// 遊戲 Code
    /// </summary>
    public string GameCode { get; set; }
    /// <summary>
    /// 離開導向網址
    /// </summary>
    public string RedirectUrl { get; set; }
    /// <summary>
    /// 是否為行動裝置
    /// </summary>
    public bool Mobile { get; set; }
    /// <summary>
    /// 語系 (en、zh、th)
    /// </summary>
    public string Lang { get; set; }
}