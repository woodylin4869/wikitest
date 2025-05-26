using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Request;
public class LogoutAllRequest
{
    /// <summary>
    /// 營運商帳號
    /// </summary>
    public string AgentName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string GameID { get; set; }
}
