using H1_ThirdPartyWalletAPI.Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game
{
    public class CheckTransferRecordResponse
    {
        public WalletTransferRecord TRecord { get; set; }
        public decimal CreditChange { get; set; }
        public decimal LockCreditChange { get; set; }
    }

}
