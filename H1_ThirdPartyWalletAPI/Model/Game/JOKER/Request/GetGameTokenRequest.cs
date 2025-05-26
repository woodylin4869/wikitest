using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;

public class GetGameTokenRequest
{
    /// <summary>
    /// PLAY – 固定值
    /// </summary>
    [Required]
    public string Method { get; set; } = "PLAY";
    /// <summary>
    /// 玩家用户名（4~20 个字母数字字符、下划线，不区分大小写）
    /// </summary>
    [Required]
    [MinLength(4)]
    [MaxLength(20)]
    public string Username { get; set; }
    /// <summary>
    /// UNIX 时间戳
    /// </summary>
    [Required]
    public long Timestamp { get; set; } = Helper.GetCurrentTimestamp();
}