using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;

public class TransferCreditRequest
{
    /// <summary>
    /// TC – 固定值
    /// </summary>
    [Required]
    public string Method { get; set; } = "TC";
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
    /// <summary>
    /// 1.正数：将金额转入提供者的系统
    /// 2.负数：将金额从提供者的系统中转出
    /// </summary>
    [Required]
    public string Amount { get; set; }
}