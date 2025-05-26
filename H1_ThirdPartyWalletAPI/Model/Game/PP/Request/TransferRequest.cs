using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.PP.Request
{
    public class TransferRequest
    {
        public string secureLogin { get; set; }
        public string externalPlayerId { get; set; }
        public string externalTransactionId { get; set; }
        public decimal amount { get; set; }
    }
}
