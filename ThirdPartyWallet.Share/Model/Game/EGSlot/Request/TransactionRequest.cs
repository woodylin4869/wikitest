using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Request;
public class TransactionRequest
{
    /// <summary>
    /// 查詢開始時間戳，單位毫秒
    /// </summary>
    public long StartTime { get; set; }
    /// <summary>
    /// 查詢結束時間戳，單位毫秒
    /// </summary>
    public long EndTime { get; set; }
    /// <summary>
    /// 運營商帳號
    /// </summary>
    public string AgentName { get; set; }
    /// <summary>
    /// 玩家帳號
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// 遊戲代號
    /// </summary>
    public string GameID { get; set; }
    /// <summary>
    /// 指定查詢主單號
    /// </summary>
    public string MainTxID { get; set; }
    /// <summary>
    /// 交易狀態、0: 進行中、1: 已完成
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// 頁數，預設為 1
    /// </summary>
    public int Page { get; set; }
    /// <summary>
    /// 每頁筆數，預設為 20
    /// </summary>
    public int PageSize { get; set; }
}
