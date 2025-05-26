using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response
{
    /// <summary>
    /// 取得遊戲帳務
    /// </summary>
    public class GetSummaryByTxTimeHourResponse : SEXYBaseStatusRespones
    {
        public List<Transaction> transactions { get; set; }

        public class Transaction
        {
            public string gameType { get; set; }
            public string gameName { get; set; }
            public string gameCode { get; set; }
            public string currency { get; set; }
            public string platform { get; set; }
            public decimal turnover { get; set; }
            public int betCount { get; set; }
            public decimal betAmount { get; set; }
            public decimal winAmount { get; set; }
            public decimal realWinAmount { get; set; }
            public decimal realBetAmount { get; set; }
            public decimal jackpotBetAmount { get; set; }
            public decimal jackpotWinAmount { get; set; }
        }

    }
}