using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 取款
/// </summary>
public class WithdrawRequest
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    public string gamehall { get; set; }

    /// <summary>
    /// 玩家帳號
    /// </summary>
    public string account { get; set; }

    /// <summary>
    /// 交易代碼
    /// 交易代碼為自行產生的唯一代碼(不可含特殊字元)，最少4碼，最多30碼
    /// </summary>
    [Required]
    [StringLength(30, MinimumLength = 4)]
    public string transaction_id { get; set; }

    /// <summary>
    /// 金額
    /// 格式需有小數點後兩位 EX:500.00 只支援到10位數
    /// </summary>
    public decimal amount { get; set; }
}
