using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Response
{
    public class WithdrawResponse
    {
        /// <summary>
        /// 交易代碼與Request相同的transaction_id
        /// </summary>
        public string transaction_id { get; set; }
        /// <summary>
        /// 遊戲帳號
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 交易金額(最多允許小數點後2位)
        /// </summary>
        public decimal amount { get; set; }
        /// <summary>
        /// 幣別 (使用ISO 4217標準)
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// 轉帳前錢包餘額
        /// </summary>
        public decimal before_balance { get; set; }
        /// <summary>
        /// 轉帳後錢包餘額
        /// </summary>
        public decimal after_balance { get; set; }
    }
}
