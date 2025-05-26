using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Request
{
    public class kickoutRequest
    {
        /// <summary>
        /// HOSTID(PS提供)
        /// </summary>
        public string host_id {  get; set; }
        /// <summary>
        /// 會員ID
        /// </summary>
        public string member_id { get; set; }
    }
}
