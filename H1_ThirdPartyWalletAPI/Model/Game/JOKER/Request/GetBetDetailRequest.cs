using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;

public class GetBetDetailRequest
{
    /// <summary>
    /// TSM – 固定值
    /// </summary>
    [Required]
    public string Method { get; set; } = "TSM";
    /// <summary>
    /// Ex: 2020-09-25 16:05
    /// </summary>
    [Required]
    public string StartDate { get; set; }
    /// <summary>
    /// Ex: 2020-09-25 16:15
    /// </summary>
    [Required]
    public string EndDate { get; set; }
    /// <summary>
    /// 如果返回的 NextId 不是空，则请求具有相同 StartDate 和 EndDate 的 NextId的 API。
    /// 重复此操作直到 NextId 是空
    /// </summary>
    public string NextId { get; set; }
    /// <summary>
    /// UNIX 时间戳
    /// </summary>
    [Required]
    public long Timestamp { get; set; } = Helper.GetCurrentTimestamp();

    /// <summary>
    /// 0 –固定值
    /// </summary>
    [Required]
    public int Delay { get; set; } = 0;
}