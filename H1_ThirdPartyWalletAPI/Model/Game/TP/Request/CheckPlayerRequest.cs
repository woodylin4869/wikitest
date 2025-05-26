using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 查詢玩家帳號是否存在
/// </summary>
public class CheckPlayerRequest
{
    /// <summary>
    /// player帳號
    /// </summary>
    [Required]
    public string account { get; set; }
}
