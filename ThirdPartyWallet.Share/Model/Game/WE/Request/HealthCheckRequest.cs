using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Request;
public class HealthCheckRequest
{
    /// </summary>
    /// 運營商ID
    /// </summary>
    public string operatorID { get; set; }

    /// <summary>
    /// WE 提供的憑證金鑰
    /// </summary>
    public string appSecret { get; set; }
}
