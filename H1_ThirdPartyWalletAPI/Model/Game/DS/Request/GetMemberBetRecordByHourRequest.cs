using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Request
{
    public class GetMemberBetRecordByHourRequest
    {
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public int Index { get; set; }
        public int limit { get; set; }
    }

}
