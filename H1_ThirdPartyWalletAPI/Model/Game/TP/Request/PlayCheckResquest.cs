using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 注單詳細資訊 PlayCheck
/// </summary>
public class PlayCheckResquest
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    [Required]
    public string gamehall { get; set; }

    /// <summary>
    /// 注單編號
    /// </summary>
    [Required]
    public string betID { get; set; }

    /// <summary>
    /// 搜尋起始時間
    /// </summary>
    [Required]
    public DateTime bet_time { get; set; }

    /// <summary>
    /// 語言代碼
    ///  en, zh-cn, zh-tw, th, ja, vi, id
    /// </summary>
    [Required]
    public string lang { get; set; }
}
