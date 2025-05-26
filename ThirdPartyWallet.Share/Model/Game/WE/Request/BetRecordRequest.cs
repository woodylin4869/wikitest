using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Request;
public class BetRecordRequest
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
    /// 玩家ID
    /// </summary>
    public string playerID { get; set; }
    /// <summary>
    /// 注單ID
    /// </summary>
    public string betID { get; set; }
    /// <summary>
    /// 遊戲組別
    /// </summary>
    public string category { get; set; }
    /// <summary>
    /// 注單狀態 (new:未結算complete:已結算 cancel:取消)
    /// </summary>
    public string betstatus { get; set; }
    /// <summary>
    /// 資料限制筆數 (預設:50 最大:500)
    /// </summary>
    public int limit { get; set; }
    /// <summary>
    /// 略過筆數 (可用於分頁)
    /// </summary>
    public int offset { get; set; }
    /// <summary>
    /// 設定為"true"時, 開始與結束時間以結算時間搜尋, 預設以投注時間搜尋
    /// </summary>
    public int isSettlementTime { get; set; }
    /// <summary>
    /// 請求時間限制兩分鐘內 (UNIX)
    /// </summary>
    public long requestTime { get; set; }
}
