using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 查詢注單
/// </summary>
public class BetLogRequest
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    [Required]
    public string gamehall { get; set; }

    /// <summary>
    /// 搜尋起始時間
    /// </summary>
    [Required]
    public DateTime start_time { get; set; }

    /// <summary>
    /// 搜尋結束時間
    /// </summary>
    [Required]
    public DateTime end_time { get; set; }

    /// <summary>
    /// 查詢頁數
    /// 最小1
    /// </summary>
    [Required]
    public int page { get; set; }

    /// <summary>
    /// 每頁顯示筆數
    /// 最小5000, 最大20000
    /// </summary>
    [Required]
    public int page_size { get; set; }
}
