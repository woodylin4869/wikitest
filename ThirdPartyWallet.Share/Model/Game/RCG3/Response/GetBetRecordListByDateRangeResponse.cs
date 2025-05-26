using System;
using System.Collections.Generic;

namespace ThirdPartyWallet.Share.Model.Game.RCG3.Response
{
    public class GetBetRecordListByDateRangeResponse
    {

        public int total { get; set; }
        public string systemCode { get; set; }
        public string webId { get; set; }
        public List<RCG3BetRecord> dataList { get; set; }
    }
}
