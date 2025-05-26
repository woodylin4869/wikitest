using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response;
public class CreateUserResponse: ResponseBase
{
    /// <summary>
    /// 玩家ID
    /// </summary>
    public string playerID { get; set; }
    /// <summary>
    /// 貨幣
    /// </summary>
    public string currency { get; set; }
    /// <summary>
    /// 時間(UNIX)
    /// </summary>
    public long time { get; set; }

}
