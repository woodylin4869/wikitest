using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Request
{
    public class StatusRequest
    {
        /// <summary>
        /// 轉帳錢包：玩家帳號，唯一值
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 運營商帳號
        /// </summary>
        public string AgentName { get; set; }
    }
}
