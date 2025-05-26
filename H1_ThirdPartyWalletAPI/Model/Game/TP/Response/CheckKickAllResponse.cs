namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;

/// <summary>
/// 查詢登出所有玩家進度
/// </summary>
public class CheckKickAllResponse
{
    /// <summary>
    /// 登出狀態
    /// </summary>
    public string ststus { get; set; }

    /// <summary>
    /// 總玩家數
    /// </summary>
    public string total { get; set; }

    /// <summary>
    /// 已登出玩家數
    /// </summary>
    public string done { get; set; }
}
