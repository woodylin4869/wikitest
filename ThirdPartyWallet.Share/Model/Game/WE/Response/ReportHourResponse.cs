using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response;
public class ReportHourResponse : ResponseBase
{
    /// <summary>
    /// 下注總額
    /// </summary>
    public decimal currencyBetAmount { get; set; }
    /// <summary>
    /// 有效投注額
    /// </summary>
    public decimal currencyValidBetAmount { get; set; }
    /// <summary>
    /// 淨輸贏
    /// </summary>
    public decimal currencyWinAmount { get; set; }
    /// <summary>
    /// USDT總下注額
    /// </summary>
    public decimal totalBetAmount { get; set; }
    /// <summary>
    /// USDT淨輸贏 ( 單位:分 )
    /// </summary>
    public decimal totalWinAmount { get; set; } 
    /// <summary>
    /// USDT有效投注額 ( 單位:分 )
    /// </summary>
    public decimal totalValidBetAmount { get; set; }
    /// <summary>
    /// 總注單數量
    /// </summary>
    public int numberOfBet { get; set; }
}
