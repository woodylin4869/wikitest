using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Request;
public class BetDetailUrlRequest
{
    /// <summary>
    /// 在 WE 註冊的營運商ID
    /// </summary>
    public string operatorID { get; set; }
    /// <summary>
    /// 注單ID
    /// </summary>
    public string betID { get; set; }
    /// <summary>
    /// 請求時間限制兩分鐘內 (UNIX)
    /// </summary>
    public long requestTime { get; set; }
    /// <summary>
    /// format=json,返回json格式的體育詳細注單資料(只限體育)
    /// </summary>
    public string format { get; set; }
}
