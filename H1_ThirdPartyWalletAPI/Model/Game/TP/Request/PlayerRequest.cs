using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 建立 Player
/// </summary>
public class PlayerRequest
{
    /// <summary>
    /// 玩家帳號
    /// a-z 0-9
    /// </summary>
    [Required]
    [StringLength(15, MinimumLength = 4)]
    public string account { get; set; }

    /// <summary>
    /// 玩家密碼
    /// a-z 0-9
    /// </summary>
    [Required]
    [StringLength(15, MinimumLength = 4)]
    public string password { get; set; }

    /// <summary>
    /// 玩家別名
    /// </summary>
    public string nickname { get; set; }
}
