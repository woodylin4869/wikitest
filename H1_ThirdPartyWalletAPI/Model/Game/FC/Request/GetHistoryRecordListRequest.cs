using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Request
{
    public class GetHistoryRecordListRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
