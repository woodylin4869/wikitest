using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.META.Response
{
    public class TransactionLogResponse : GetMetaDataDecryptBase
    {
        public int totalRows { get; set; }
        public int overRows { get; set; }
        public int limit { get; set; }
        public Row[] rows { get; set; }

        public class Row
        {
            public string Account { get; set; }
            public string TranPoint { get; set; }
            public DateTime DateTran { get; set; }
            public long TranOrder { get; set; }
            public string TradeOrder { get; set; }
        }
    }
}
