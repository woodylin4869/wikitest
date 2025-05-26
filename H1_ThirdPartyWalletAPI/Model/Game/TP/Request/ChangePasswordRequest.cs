using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 玩家更換密碼
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// 玩家帳號
    /// </summary>
    [Required]
    public string account { get; set; }


    /// <summary>
    /// 密碼
    /// </summary>
    [Required]
    public string password { get; set; }
}
