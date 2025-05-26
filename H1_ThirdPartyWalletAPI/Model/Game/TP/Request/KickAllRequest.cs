using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 登出所有玩家
/// </summary>
public class KickAllRequest
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    [Required]
    public string gamehall { get; set; }
}
