using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;


/// <summary>
/// 建立 Player
/// </summary>
public class PlayerResponse
{
    /// <summary>
    /// 玩家帳號
    /// </summary>
    public string account { get; set; }

    /// <summary>
    /// 玩家名稱
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 代理商帳號
    /// </summary>
    public string agent_account { get; set; }

    /// <summary>
    /// 代理商代碼
    /// </summary>
    public string agent_code { get; set; }

    /// <summary>
    /// 娛樂城錢包
    /// </summary>
    public Dictionary<string, decimal> casino_wallet { get; set; }
}
