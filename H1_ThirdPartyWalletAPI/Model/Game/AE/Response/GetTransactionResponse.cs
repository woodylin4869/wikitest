using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Response
{
    public class GetTransactionResponse : AEResponseBase
    {
        public string type { get; set; }
        public string tx_id { get; set; }
        public string account_name { get; set; }
        public decimal amount { get; set; }
        public decimal end_balance { get; set; }
        public string state { get; set; }
        public DateTime tx_time { get; set; }

    }

}
