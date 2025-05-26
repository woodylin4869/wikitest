using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Request
{
    public class BetHistoryRecordRequest
    {
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public int pageIndex { get; set; }
    }
}
