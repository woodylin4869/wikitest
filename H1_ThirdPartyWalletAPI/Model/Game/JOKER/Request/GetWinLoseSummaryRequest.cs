using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;

public class GetWinLoseSummaryRequest
{
    /// <summary>
    /// RWL – 固定值
    /// </summary>
    [Required]
    public string Method { get; set; } = "RWL";
    /// <summary>
    /// Ex: 2020-09-25
    /// </summary>
    [Required]
    public string StartDate { get; set; }
    /// <summary>
    /// Ex: 2020-09-26
    /// </summary>
    [Required]
    public string EndDate { get; set; }
    /// <summary>
    /// 根据用户名筛选
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// UNIX 时间戳
    /// </summary>
    [Required]
    public long Timestamp { get; set; } = Helper.GetCurrentTimestamp();
}