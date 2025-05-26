using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RCG2.Response
{
    public class GetBetRecordListByDateRangeResponse
    {

        public int total { get; set; }
        public string systemCode { get; set; }
        public string webId { get; set; }
        public List<RCG2BetRecord> dataList { get; set; }
    }
}
