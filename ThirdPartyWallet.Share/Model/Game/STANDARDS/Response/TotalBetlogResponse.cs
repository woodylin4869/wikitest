using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Response
{
    public class TotalBetlogResponse
    {
        /// <summary>
        /// 總投注總金額
        /// </summary>
        public decimal bet_amount { get; set; }
        /// <summary>
        /// 有效投注總金額
        /// </summary>
        public decimal bet_valid_amount { get; set; }
        /// <summary>
        /// 派彩金額(純贏分)
        /// </summary>
        public decimal pay_off_amount { get; set; }
        /// <summary>
        /// 彩金獲得金額
        /// </summary>
        public decimal jp_win { get; set; }
        /// <summary>
        /// 注單數量
        /// </summary>
        public int Bet_quantity { get; set; }
    }
}
