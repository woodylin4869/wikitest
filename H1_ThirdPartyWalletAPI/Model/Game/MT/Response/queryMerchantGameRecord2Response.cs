using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Response
{
    public class queryMerchantGameRecord2Response
    {
        
            public string resultCode { get; set; }
            public List<Translist> transList { get; set; }
        

        public class Translist
        {
            public string rowID { get; set; }
            public string playerName { get; set; }
            public DateTime gameDate { get; set; }
            public string gameCode { get; set; }
            public string gameType { get; set; }
            public string period { get; set; }
            public decimal betAmount { get; set; }
            public decimal winAmount { get; set; }
            public decimal commissionable { get; set; }
            public decimal roomFee { get; set; }
            public decimal income { get; set; }
            public string timeZone { get; set; }
            public decimal progressive_wins { get; set; }
            public decimal progressive_share { get; set; }
            public string merchantId { get; set; }
            public string currency { get; set; }
            public string recordID { get; set; }
            public Guid summary_id { get; set; }
            public DateTime partition_time { get; set; }
            public DateTime report_time { get; set; }

        }

    }
}
