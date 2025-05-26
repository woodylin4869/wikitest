using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response;

public class DepositResponse:ResponseBase
{
    /// <summary>
    /// 餘額(單位:分)
    /// </summary>
    public decimal balance { get; set; }
    /// <summary>
    /// 貨幣
    /// </summary>
    public string currency { get; set; }
    /// <summary>
    /// 時間(UNIX)
    /// </summary>
    public long time { get; set; }
    /// <summary>
    /// WE 交易編號
    /// </summary>
    public string refID { get; set; }

}
