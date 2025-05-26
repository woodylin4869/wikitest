using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;

public class GetCreditRequest
{
    /// <summary>
    /// GC – 固定值
    /// </summary>
    [Required]
    public string Method { get; set; } = "GC";
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
}