using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response
{
    /// <summary>
    /// 取得遊戲帳務
    /// </summary>
    public class GetTransactionByUpdateDateResponse : SEXYBaseStatusRespones
    {
        public List<Record> transactions { get; set; }
    }

    public class Record
    {
        public Guid summary_id { get; set; }
        public string gameType { get; set; }
        public decimal winAmount { get; set; }
        public DateTime txTime { get; set; }
        public int settleStatus { get; set; }
        public string gameInfo { get; set; }
        public decimal realWinAmount { get; set; }
        public DateTime updateTime { get; set; }
        public decimal realBetAmount { get; set; }
        public string userId { get; set; }
        public string betType { get; set; }
        public string platform { get; set; }
        public int txStatus { get; set; }
        public decimal betAmount { get; set; }
        public string gameName { get; set; }
        public string platformTxId { get; set; }
        public DateTime betTime { get; set; }
        public string gameCode { get; set; }
        public string currency { get; set; }
        public decimal jackpotWinAmount { get; set; }
        public decimal jackpotBetAmount { get; set; }
        public decimal turnover { get; set; }
        public string roundId { get; set; }

        public decimal pre_betAmount { get; set; }
        public decimal pre_realWinAmount { get; set; }
        public decimal pre_turnover { get; set; }
        public decimal pre_realBetAmount { get; set; }
    }
}