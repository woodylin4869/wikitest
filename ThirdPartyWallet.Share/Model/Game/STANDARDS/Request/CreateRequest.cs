using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Request
{
    public class CreateRequest
    {
        /// <summary>
        /// 遊戲帳號
        /// </summary>
        public string account {  get; set; }
        /// <summary>
        /// 玩家別名
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 幣別
        /// </summary>
        public string currency {  get; set; }
    }
}
