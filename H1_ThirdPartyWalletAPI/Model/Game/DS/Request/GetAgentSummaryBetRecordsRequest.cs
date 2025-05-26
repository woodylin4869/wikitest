using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Request
{
    public class GetAgentSummaryBetRecordsRequest
    {
        public FinishTime finish_time { get; set; }
        public class FinishTime
        {
            public DateTime start_time { get; set; }
            public DateTime end_time { get; set; }
        }
    }
}