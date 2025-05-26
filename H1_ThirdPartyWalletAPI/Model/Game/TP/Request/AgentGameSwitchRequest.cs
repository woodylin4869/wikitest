using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 代理商遊戲開關
/// </summary>
public class AgentGameSwitchRequest
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    [Required]
    public string gamehall { get; set; }

    /// <summary>
    /// 遊戲代碼
    /// 若未填則更新該gamehall所有遊戲
    /// 查詢多個請用‘,’分隔
    /// </summary>
    public string gamecode { get; set; }

    /// <summary>
    /// 遊戲開關切換
    /// ‘open’ => 開啟, ‘close’ => 關閉
    /// </summary>
    [Required]
    public string switch_type { get; set; }

}
