using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 玩家遊戲錢包查詢
/// </summary>
public class PlayerWalletRequest
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    [Required]
    public string gamehall { get; set; }

    /// <summary>
    /// 玩家帳號
    /// </summary>
    [Required]
    public string account { get; set; }

    /// <summary>
    /// 代理id
    /// 目前只有樂利彩票的遊戲有使用agent_id參數 [Required]
    /// </summary>
    public string agent_id { get; set; }
}
