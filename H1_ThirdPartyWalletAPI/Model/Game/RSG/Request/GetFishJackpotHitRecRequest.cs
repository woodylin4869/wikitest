using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;

/// <summary>
/// 取得捕魚機Jackpot 中獎紀錄
/// </summary>
public class GetFishJackpotHitRecRequest
{
    /// <summary>
    /// 系統代碼(只限英數)
    /// </summary>
    [MinLength(2)]
    [MaxLength(20)]
    [Required]
    public string SystemCode { get; set; }
    /// <summary>
    /// 站台代碼(只限英數)
    /// </summary>
    [MinLength(0)]
    [MaxLength(20)]
    [Required]
    public string WebId { get; set; }
    /// <summary>
    /// 開始時間(yyyy-MM-dd)
    /// </summary>
    [StringLength(10)]
    [Required]
    public string DateStart { get; set; }
    /// <summary>
    /// 結束時間(yyyy-MM-dd)
    /// </summary>
    [StringLength(10)]
    [Required]
    public string DateEnd { get; set; }
}
