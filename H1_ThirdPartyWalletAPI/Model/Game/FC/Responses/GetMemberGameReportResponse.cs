using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Response
{
    public class GetMemberGameReportResponse : FCBaseStatusRespones
    {
        public List<Report> Report { get; set; }


    }
    public class Report
    {
        public string account { get; set; }
        public int gameID { get; set; }
        public int gameType { get; set; }
        public decimal bet { get; set; }
        public decimal jptax { get; set; }
        public decimal win { get; set; }
        public decimal jppoints { get; set; }
        public decimal netWin { get; set; }
        public int round { get; set; }
        public int betCount { get; set; }
        public decimal refund { get; set; }
        public decimal validBet { get; set; }
        public decimal commission { get; set; }
    }

}
