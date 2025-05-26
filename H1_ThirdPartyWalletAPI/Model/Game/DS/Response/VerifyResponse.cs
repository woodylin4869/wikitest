using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Response
{
    public class VerifyResponse : ResponseBaseModel<TransactionMessageResult>
    {
        public string trans_id { get; set; }
        public string serial { get; set; }
        public string amount { get; set; }
    }

}
