using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Request
{
    public class TransferRequest : RequestBaseModel
    {
        public Guid serial { get; set; }
        public string account { get; set; }
        public string amount { get; set; }
        public TransactionType oper_type { get; set; }
    }

    public enum TransactionType {
        Withdraw = 0,
        Deposit = 1
    }

}
