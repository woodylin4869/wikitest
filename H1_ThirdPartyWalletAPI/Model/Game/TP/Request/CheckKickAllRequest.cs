using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request;

/// <summary>
/// 查詢登出所有玩家進度
/// </summary>
public class CheckKickAllRequest
{
    /// <summary>
    /// 查詢序號
    /// 請使用 登出所有玩家 (kickall) api回覆的查詢序號check_key查詢處理進度
    /// </summary>
    [Required]
    public string check_key { get; set; }
}
