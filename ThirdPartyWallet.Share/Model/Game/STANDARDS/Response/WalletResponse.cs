using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Response
{
    public class WalletResponse
    {
        /// <summary>
        /// 會員帳號
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 餘額
        /// </summary>
        public float balance { get; set; }
        /// <summary>
        /// 幣別
        /// </summary>
        public string currency { get; set; }
    }
}
