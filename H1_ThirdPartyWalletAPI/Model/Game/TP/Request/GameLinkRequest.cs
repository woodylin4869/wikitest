using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 取得遊戲連結
/// </summary>
public class GameLinkRequest
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    [Required]
    public string gamehall { get; set; }

    /// <summary>
    /// 遊戲代碼
    /// </summary>
    [Required]
    public string gamecode { get; set; }

    /// <summary>
    /// 帳戶名稱
    /// </summary>
    [Required]
    public string account { get; set; }

    /// <summary>
    /// 語言代碼
    /// en, zh-cn, zh-tw, th, ja, vi, id
    /// </summary>
    [Required]
    public string lang { get; set; }

    /// <summary>
    /// 平台類型
    /// web, mobile
    /// </summary>
    public string platform { get; set; }

    /// <summary>
    /// 子遊戲商
    /// </summary>
    public string sub_gamehall { get; set; }

    /// <summary>
    /// 遊戲退出連結(預設為關閉視窗)
    /// </summary>
    public string return_url { get; set; }

    /// <summary>
    /// 是否為試玩
    /// 0 => false, 1 => true, 預設為0
    /// </summary>
    public string is_free_trial { get; set; }

    /// <summary>
    /// 串接平台的註冊連結
    /// </summary>
    public string register_url { get; set; }

    /// <summary>
    /// 遊戲房間id
    /// </summary>
    public string room_id { get; set; }

    /// <summary>
    /// 代理id
    /// </summary>
    public string agent_id { get; set; }

    /// <summary>
    /// 獎金組
    /// </summary>
    public string bounsmode { get; set; }
}
