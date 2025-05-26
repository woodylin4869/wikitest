using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Response
{
    public class TransferResponse : ResponseBaseModel<TransactionMessageResult>
    {
        public string trans_id { get; set; }
        public Guid serial { get; set; }

        public string balance { get; set; }
    }

   

 
}
