using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Response
{
    public class ReportHourResponse
    {
        /// <summary>
        /// 下注筆數
        /// </summary>
        public int Bet_count { get; set; }

        /// <summary>
        /// 投注總額
        /// </summary>
        public decimal Bet_total { get; set; }

        /// <summary>
        /// 有效投注總額
        /// </summary>
        public decimal Bet_real { get; set; }

        /// <summary>
        /// 輸贏總額
        /// </summary>
        public decimal Payoff { get; set; }

        /// <summary>
        /// 彩金總額
        /// </summary>
        public decimal Jackpot { get; set; }
    }
}