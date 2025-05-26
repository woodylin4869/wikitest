using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Response
{
    public class StatusResponse : ErrorCodeResponse
    {
        /// <summary>
        /// 玩家幣別。大寫
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 玩家餘額
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 玩家保留餘額
        /// </summary>
        public string Reserved { get; set; }
        /// <summary>
        /// 是否在線
        /// </summary>
        public string IsOnline { get; set; }
    }
}
