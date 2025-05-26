using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response;
public class BalanceResponse : ResponseBase
{
    /// <summary>
    /// 餘額
    /// </summary>
    public decimal balance { get; set; }
    /// <summary>
    /// 幣別
    /// </summary>
    public string currency { get; set; }
    /// <summary>
    /// 時間(UNIX)
    /// </summary>
    public long time { get; set; }
    
}
