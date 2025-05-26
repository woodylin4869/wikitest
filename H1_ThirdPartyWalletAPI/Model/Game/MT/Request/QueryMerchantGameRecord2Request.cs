

using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Request
{
    public class QueryMerchantGameRecord2Request
    {
        public string merchantId { get; set; }
        public string data { get; set; }
    }
    public class QueryMerchantGameRecord2rawData
    {
        public long recordID { get; set; }
        public string gameType { get; set; }
        public string? startTime { get; set; }
        public string? endTime { get; set; }
        public string currency { get; set; }
    }

}
