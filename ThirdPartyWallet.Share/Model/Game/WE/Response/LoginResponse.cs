using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response;
public class LoginResponse:ResponseBase
{
    /// <summary>
    /// 遊戲URL
    /// </summary>
    public string url { get; set; }
    /// <summary>
    /// 玩家ID
    /// </summary>
    public string playerID { get; set; }
    /// <summary>
    /// 時間(UNIX)
    /// </summary>
    public long time { get; set; }

}
