using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Response
{
    public class HourbetlogResponse
    {
        /// <summary>
        /// 會員帳號
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 總下注金額
        /// </summary>
        public float bet_amount { get; set; }
        /// <summary>
        /// 總有效下注金額
        /// </summary>
        public float bet_valid_amount { get; set; }
        /// <summary>
        /// 派彩金額 (純贏分)
        /// </summary>
        public float pay_off_amount { get; set; }
        /// <summary>
        /// 彩金獲得金額
        /// </summary>
        public float jp_win { get; set; }
        /// <summary>
        /// 住單數量
        /// </summary>
        public int Bet_quantity { get; set; }
    }
}
