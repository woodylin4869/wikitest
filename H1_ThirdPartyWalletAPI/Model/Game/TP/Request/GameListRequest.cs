namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 遊戲列表
/// </summary>
public class GameListRequest
{
    /// <summary>
    /// 遊戲廠商 
    /// 若未填就撈全部
    /// </summary>
    public string gamehall { get; set; }
}
