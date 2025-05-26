using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Request
{
    public class AuthorizeRequest
    {
        /// <summary>
        /// 會員token
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// 目前認證狀態
        /// </summary>
        public int step { get; set; }
    }
}
