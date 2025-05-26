using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;

/// <summary>
/// 遊戲列表
/// </summary>
public class GameListResponse
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    public string gamehall { get; set; }

    /// <summary>
    /// 遊戲代碼
    /// </summary>
    public string gamecode { get; set; }

    /// <summary>
    /// 子遊戲廠商代碼
    /// </summary>
    public string sub_gamehall { get; set; }

    /// <summary>
    /// 遊戲名稱
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 遊戲logo
    /// </summary>
    public Dictionary<string, string> image_urls { get; set; }
}
