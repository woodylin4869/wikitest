using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 查詢投注量統計
/// 查詢區間最小為1小時，不支援分秒查詢
/// </summary>
public class StatisticsRequest
{
    /// <summary>
    /// 搜尋起始時間
    /// 格式為：YYYY-MM-DD H:00:00
    /// 例：2021-05-20 23:00:00
    /// </summary>
    [Required]
    public DateTime start_time { get; set; }

    /// <summary>
    /// 搜尋結束時間
    /// 格式為：YYYY-MM-DD H:59:59
    /// 例：2021-05-20 23:59:59
    /// </summary>
    [Required]
    public DateTime end_time { get; set; }

    /// <summary>
    /// 時區
    /// 可代入-04、+00、+08
    /// 預設為 -04
    /// </summary>
    public string timezone { get; set; }
}
