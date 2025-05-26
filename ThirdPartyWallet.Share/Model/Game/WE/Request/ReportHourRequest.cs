using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Request;
public class ReportHourRequest
{
    /// <summary>
    /// 在 WE 註冊的營運商ID
    /// </summary>
    public string operatorID { get; set; }
    /// <summary>
    /// 搜尋開始時間 (UNIX), 與結束時間間隔不得大於一個月
    /// </summary>
    public long startTime { get; set; }
    /// <summary>
    /// 搜尋結束時間 (UNIX), 與開始時間間隔不得大於一個月
    /// </summary>
    public long endTime { get; set; }
    /// <summary>
    /// 設定為"true"時, 開始與結束時間以結算時間搜尋, 預設以投注時間搜尋
    /// </summary>
    public int isSettlementTime { get; set; }
    /// <summary>
    /// 請求時間限制兩分鐘內 (UNIX)
    /// </summary>
    public long requestTime { get; set; }

}
