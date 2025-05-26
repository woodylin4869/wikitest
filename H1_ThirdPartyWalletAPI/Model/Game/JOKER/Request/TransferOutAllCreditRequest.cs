using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;

public class TransferOutAllCreditRequest
{
    /// <summary>
    /// WAC – 固定值
    /// </summary>
    [Required]
    public string Method { get; set; } = "WAC";
    /// <summary>
    /// 玩家用户名
    /// </summary>
    [Required]
    public string Username { get; set; }
    /// <summary>
    /// UNIX 时间戳
    /// </summary>
    [Required]
    public long Timestamp { get; set; } = Helper.GetCurrentTimestamp();
    /// <summary>
    /// 这是一个唯一的密钥，用于验证转账到提供者系统的金额（字母数字字符。最大长度是 50）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string RequestID { get; set; }
}