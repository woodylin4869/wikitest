using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Response;

public class TransactionResponse : ErrorCodeResponse
{
    public Datum[] Data { get; set; }
    /// <summary>
    /// 是否有下一頁
    /// </summary>
    public bool Next { get; set; }
}
public class Datum
{
    /// <summary>
    /// 指定查詢主單號
    /// </summary>
    public string MainTxID { get; set; }
    /// <summary>
    /// 遊戲代號
    /// </summary>
    public string GameID { get; set; }
    /// <summary>
    /// 玩家帳號
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// 幣別
    /// </summary>
    public string Currency { get; set; }
    /// <summary>
    /// 注單類型、0: 一般注單、1: 莊家注單、2: 閒家注單
    /// </summary>
    public int BetType { get; set; }
    /// <summary>
    /// 下注時間戳，單位毫秒
    /// </summary>
    public long BetTime { get; set; }
    /// <summary>
    /// 派彩時間戳，單位毫秒
    /// </summary>
    public long WinTime { get; set; }
    /// <summary>
    /// 下注金額
    /// </summary>
    public decimal Bet { get; set; }
    /// <summary>
    /// 派彩金額
    /// </summary>
    public decimal Win { get; set; }
    /// <summary>
    /// 淨贏金額
    /// </summary>
    public decimal NetWin { get; set; }
    /// <summary>
    /// 交易狀態、0: 進行中、1: 已完成
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// 遊玩前，玩家餘額
    /// </summary>
    public decimal BeforeBalance { get; set; }
    /// <summary>
    /// 遊玩後，玩家餘額
    /// </summary>
    public decimal AfterBalance { get; set; }
    /// <summary>
    /// 下注金額(前一狀態)
    /// </summary>
    public decimal Pre_Bet { get; set; }
    /// <summary>
    /// 派彩金額(前一狀態)
    /// </summary>
    public decimal Pre_Win { get; set; }
    /// <summary>
    /// 淨贏金額(前一狀態)
    /// </summary>
    public decimal Pre_NetWin { get; set; }
    /// <summary>
    /// 彙總時間
    /// </summary>
    public DateTime report_time { get; set; }
    /// <summary>
    /// Club_id (running表)
    /// </summary>
    public string Club_id { get; set; }

    /// <summary>
    /// Franchiser_id (running表)
    /// </summary>
    public string Franchiser_id { get; set; }
}


public class W1Datum
{
    /// <summary>
    /// 指定查詢主單號
    /// </summary>
    public string MainTxID { get; set; }
    /// <summary>
    /// 遊戲代號
    /// </summary>
    public string GameID { get; set; }
    /// <summary>
    /// 玩家帳號
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// 幣別
    /// </summary>
    public string Currency { get; set; }
    /// <summary>
    /// 注單類型、0: 一般注單、1: 莊家注單、2: 閒家注單
    /// </summary>
    public int BetType { get; set; }
    /// <summary>
    /// 下注時間戳，單位毫秒
    /// </summary>
    public DateTime BetTime { get; set; }
    /// <summary>
    /// 派彩時間戳，單位毫秒
    /// </summary>
    public DateTime WinTime { get; set; }
    /// <summary>
    /// 下注金額
    /// </summary>
    public decimal Bet { get; set; }
    /// <summary>
    /// 派彩金額
    /// </summary>
    public decimal Win { get; set; }
    /// <summary>
    /// 淨贏金額
    /// </summary>
    public decimal NetWin { get; set; }
    /// <summary>
    /// 交易狀態、0: 進行中、1: 已完成
    /// </summary>
    public int Status { get; set; }
    /// <summary>
    /// 遊玩前，玩家餘額
    /// </summary>
    public decimal BeforeBalance { get; set; }
    /// <summary>
    /// 遊玩後，玩家餘額
    /// </summary>
    public decimal AfterBalance { get; set; }
    /// <summary>
    /// 下注金額(前一狀態)
    /// </summary>
    public decimal Pre_Bet { get; set; }
    /// <summary>
    /// 派彩金額(前一狀態)
    /// </summary>
    public decimal Pre_Win { get; set; }
    /// <summary>
    /// 淨贏金額(前一狀態)
    /// </summary>
    public decimal Pre_NetWin { get; set; }
    /// <summary>
    /// 彙總時間
    /// </summary>
    public DateTime report_time { get; set; }
}

