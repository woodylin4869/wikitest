using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Response
{
    public class PlaycheckResponse
    {
        /// <summary>
        /// 注單號
        /// </summary>
        public string bet_id {  get; set; }
        /// <summary>
        /// URL
        /// </summary>
        public string result { get; set; }
    }
}
