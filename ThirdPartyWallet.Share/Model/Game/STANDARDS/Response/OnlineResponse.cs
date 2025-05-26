using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Response
{
    public class OnlineResponse
    {
        /// <summary>
        /// 遊戲帳號
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 是否在線 
        /// </summary>
        public bool online { get; set; }
    }
}
