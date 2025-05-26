namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;

public class PlayerWalletResponse
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    public string gamehall { get; set; }

    /// <summary>
    /// 玩家遊戲錢包餘額
    /// </summary>
    public string balance { get; set; }
}
