namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;

/// <summary>
/// 遊戲廠商列表
/// </summary>
public class GamehallsResponse
{
    /// <summary>
    /// 遊戲廠商簡稱
    /// </summary>
    public string gamehall { get; set; }

    /// <summary>
    /// 遊戲廠商全名
    /// </summary>
    public string fullname { get; set; }

    /// <summary>
    /// 遊戲廠商狀態
    /// </summary>
    public bool is_open { get; set; }
}
