using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Request;
public class SetbetlimitRequest
{
    /// <summary>
    /// 運營商ID
    /// </summary>
    public string operatorID { get; set; }
    /// <summary>
    /// 玩家ID
    /// </summary>
    public string playerID { get; set; }
    /// <summary>
    /// 限注範例:A1,A2,A3 (最多三組)
    /// </summary>
    public string betlimit { get; set; }
    /// <summary>
    /// 請求時間限制兩分鐘內 (UNIX)
    /// </summary>
    public long requestTime { get; set; }
}
