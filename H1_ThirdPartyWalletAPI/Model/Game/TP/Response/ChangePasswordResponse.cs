using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;

/// <summary>
/// 玩家更換密碼
/// </summary>
public class ChangePasswordResponse
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    public string gamehall { get; set; }

    /// <summary>
    /// 更換密碼執行狀態
    /// </summary>
    public string data { get; set; }
}
