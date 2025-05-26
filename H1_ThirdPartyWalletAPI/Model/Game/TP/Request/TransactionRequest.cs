using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 多筆交易紀錄查詢
/// </summary>
public class TransactionRequest
{
    /// <summary>
    /// 搜尋起始時間
    /// 時間時區為 UTC-4 美東時間
    /// API內若有包含開始時間(starttime)與結束時間(endtime)參數,該時間皆包含在查詢範圍 
    /// </summary>
    [Required]
    public DateTime start_time { get; set; }

    /// <summary>
    /// 搜尋結束時間
    /// 時間時區為 UTC-4 美東時間
    /// API內若有包含開始時間(starttime)與結束時間(endtime)參數,該時間皆包含在查詢範圍 
    /// </summary>
    [Required]
    public DateTime end_time { get; set; }
}
