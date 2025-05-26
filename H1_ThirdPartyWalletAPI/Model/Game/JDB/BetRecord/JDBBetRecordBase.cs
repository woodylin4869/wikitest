using H1_ThirdPartyWalletAPI.Model.Game.JDB;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Enum;
using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class JDBBetRecordBase
    {
        public JDBBetRecordBase()
        {
        }

        public JDBBetRecordBase(CommonBetRecord betRecord)
        {
            this.SessionId = betRecord.SessionId;
            this.seqNo = Convert.ToInt64(betRecord.historyId);
            this.historyId = betRecord.historyId;
            this.playerId = betRecord.playerId;
            this.gType = betRecord.gType;
            this.mtype = betRecord.mtype;
            this.gameDate = betRecord.gameDate;
            this.bet = betRecord.bet;
            this.win = betRecord.win;
            this.total = betRecord.total;
            this.currency = betRecord.currency;
            this.denom = betRecord.denom;
            this.lastModifyTime = betRecord.lastModifyTime;
            this.playerIp = betRecord.playerIp;
            this.clientType = betRecord.clientType;
        }

        public int SessionId { get; set; }

        // 2024-04-25 JDB預計移除；W1超前部屬先行移除
        public long seqNo { get; set; }

        /// <summary>
        /// 集成遊戲商提供的遊戲序號
        /// </summary>
        public string historyId { get; set; }

        public string playerId { get; set; }
        public GameType gType { get; set; }
        public int mtype { get; set; }
        public DateTime gameDate { get; set; }
        public decimal bet { get; set; }

        public decimal win { get; set; }
        public decimal total { get; set; }
        public string currency { get; set; }
        public decimal denom { get; set; }
        public DateTime lastModifyTime { get; set; }
        public string playerIp { get; set; }
        public string clientType { get; set; }
        public decimal beforeBalance { get; set; }
        public decimal afterBalance { get; set; }

        /// <summary>
        /// 彙總帳時間
        /// </summary>
        public DateTime report_time { get; set; }
    }
}