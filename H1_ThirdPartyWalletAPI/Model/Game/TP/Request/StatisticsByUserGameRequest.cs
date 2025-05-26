using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 查詢投注量統計_依玩家與遊戲
/// </summary>
public class StatisticsByUserGameRequest
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

    /// <summary>
    /// 排序欄位
    /// 可代入投 注統計資料 之欄位
    /// 預設為 gamehall
    /// </summary>
    public string sort_by { get; set; }

    /// <summary>
    /// 依升序/降序排序
    /// asc : 升序, desc : 降序
    /// 預設為 asc
    /// </summary>
    public string sort_order { get; set; }

    /// <summary>
    /// 查詢頁數
    /// 最小1
    /// </summary>
    [Required]
    public int page { get; set; }

    /// <summary>
    /// 每頁顯示筆數
    /// 允許15-500
    /// </summary>
    [Required]
    public short page_size { get; set; }
}
