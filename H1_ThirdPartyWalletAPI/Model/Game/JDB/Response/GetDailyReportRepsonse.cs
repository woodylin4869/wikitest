using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class GetDailyReportRepsonse : ResponseBaseModel
    {
        public List<DaliyReportContent> Data { get; set; }
    }
    public class DaliyReportContent {
        public string Uid { get; set; }
        public decimal bet { get; set; }
        public decimal win { get; set; }
        public decimal netWin { get; set; }
        public decimal jackpot { get; set; }
        public decimal jackpotContribute { get; set; }
        public int count { get; set; }
        public decimal validBet { get; set; }
        public decimal tax { get; set; }
        public DateTime financialdate { get; set; }
        public int gtype { get; set; }
    }
}
