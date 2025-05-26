using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel;

public class t_joker_bet_record
{

    //public Guid Summary_id { get; set; }
    /// <summary>
    /// 彙總帳時間
    /// </summary>
    public DateTime report_time { get; set; }
    /// <summary>
    /// 彩金
    /// </summary>
    public decimal JackpotWin { get; set; }

    public string Ocode { get; set; }
    public string Username { get; set; }
    public string Gamecode { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public decimal Result { get; set; }
    public DateTime Time { get; set; }

    public string Roundid { get; set; }
    public string Transactionocode { get; set; }
    public BetTypeEnum BetType { get; set; }
    public DateTime Partition_time { get; set; }
}

public enum BetTypeEnum
{
    /// <summary>
    /// 一般注單
    /// </summary>
    Game,
    /// <summary>
    /// 彩金
    /// </summary>
    Jackpot,
    /// <summary>
    /// 競賽
    /// </summary>
    Competition,
}