using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;

public class GetGameHistoryUrlRequest
{
    /// <summary>
    /// History – 固定值
    /// </summary>
    [Required]
    public string Method { get; set; } = "History";
    /// <summary>
    /// 游戏事务代码（遊戲注單號）
    /// </summary>
    [Required]
    public string OCode { get; set; }
    /// <summary>
    /// 玩家的首选语言（ISO 639-1，包含 2 个字母的代码） 
    /// 例如 en、zh、th
    /// </summary>
    [Required]
    public string Language { get; set; }
    /// <summary>
    /// 事务类型。例如 Game
    /// </summary>
    [Required]
    public string Type { get; set; }
    /// <summary>
    /// UNIX 时间戳
    /// </summary>
    [Required]
    public long Timestamp { get; set; } = Helper.GetCurrentTimestamp();
}