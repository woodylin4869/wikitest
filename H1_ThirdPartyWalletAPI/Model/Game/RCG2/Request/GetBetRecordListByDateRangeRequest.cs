using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.RCG2.Request
{
    public class GetBetRecordListByDateRangeRequest: BaseRequest
    {
        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }

        public int pageIndex { get; set; }

        public int pageSize { get; set; }
    }
}
