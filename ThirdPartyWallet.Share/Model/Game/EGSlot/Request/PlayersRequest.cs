using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Request;
public class PlayersRequest
{
    /// <summary>
    /// 會員名稱
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// 幣別
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// 用戶商
    /// </summary>
    public string AgentName { get; set; }
}
