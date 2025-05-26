using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Request
{
    public class CreateuserRequest
    {
        /// <summary>
        /// host id(PS提供)
        /// </summary>
        public string host_id { get; set; }
        /// <summary>
        /// 用戶ID
        /// </summary>
        public string member_id { get; set; }
        /// <summary>
        /// 錢包類型  (選用)
        /// 0   一般（默设值） 
        /// 1   捕鱼机（仅适用于共享钱包）
        /// </summary>
        public int purpose { get; set; }
    }
}
