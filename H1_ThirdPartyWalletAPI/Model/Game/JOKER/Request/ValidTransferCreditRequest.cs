using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;

public class ValidTransferCreditRequest
{
    /// <summary>
    /// TCH – 固定值
    /// </summary>
    [Required]
    public string Method { get; set; } = "TCH";
    /// <summary>
    /// UNIX 时间戳
    /// </summary>
    [Required]
    public long Timestamp { get; set; } = Helper.GetCurrentTimestamp();
    /// <summary>
    /// 需要验证 RequestID
    /// </summary>
    [Required]
    public string RequestID { get; set; }
}