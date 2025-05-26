using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.IDN.Request
{
    public class DepositRequest
    {
        /// <summary>
        /// 充值金額（正數）
        /// </summary>
        public decimal amount { get; set; }
        public string destination_bank { get; set; }
        public int payment_id { get; set; }
        public string order_id { get; set; }
        public int is_mobile { get; set; }

    }

}