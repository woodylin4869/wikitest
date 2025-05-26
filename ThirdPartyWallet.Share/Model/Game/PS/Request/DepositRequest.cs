using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Request
{
    public class DepositRequest
    {
        /// <summary>
        /// hostid (PS提供)
        /// </summary>
        public string host_id { get; set; }
        /// <summary>
        /// 會員ID
        /// </summary>
        public string member_id { get; set; }
        /// <summary>
        /// 單號
        /// </summary>
        public string txn_id { get; set; }
        /// <summary>
        /// 總金額
        /// </summary>
        public decimal amount { get; set; }
    }
}
