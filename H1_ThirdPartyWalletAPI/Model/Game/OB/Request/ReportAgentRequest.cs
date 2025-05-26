using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Request
{
    public class ReportAgentRequest
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int pageIndex { get; set; }
    }
}
