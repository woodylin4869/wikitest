using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 代理商取得彩票後台連結
/// </summary>
public class LotteryAdministrationLinkRequest
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    [Required]
    public string gamehall { get; set; }

    /// <summary>
    /// 代理id
    /// 目前只有樂利彩票的遊戲有使用agent_id參數 [Required]
    /// </summary>
    public string agent_id { get; set; }
}
