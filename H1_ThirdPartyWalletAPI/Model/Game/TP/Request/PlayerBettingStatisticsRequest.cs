using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 查詢玩家投注量統計
/// </summary>
public class PlayerBettingStatisticsRequest
{
    /// <summary>
    /// 玩家帳號
    /// a-z 0-9
    /// </summary>
    [Required]
    public string account { get; set; }

    /// <summary>
    /// 開始時間
    /// 格式為：YYYY-MM-DD H:00:00
    /// 例：2021-05-20 23:00:00
    /// </summary>
    public DateTime start_time { get; set; }

    /// <summary>
    /// 結束時間
    /// 格式為：YYYY-MM-DD H:59:59
    /// 例：2021-05-20 23:59:59
    /// </summary>
    public DateTime end_time { get; set; }

    /// <summary>
    /// 時區
    /// 預設為 -04
    /// </summary>
    public string timezone { get; set; }
}
