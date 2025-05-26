using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class Wallet
    {
        public decimal Lock_credit { get; set; }
        public decimal Credit { get; set; }
        public string Club_Ename { get; set; }
        public string Club_id { get; set; }
        public string Currency { get; set; }
        public string Franchiser_id { get; set; }
        public decimal stop_balance { get; set; }
        public string last_platform { get; set; }
    }
}
