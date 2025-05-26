using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Request
{
    public class TotalBetlogRequest
    {
        /// <summary>
        /// 搜尋起始時間(GMT+8)
        /// </summary>
        public string start_time {  get; set; }
        /// <summary>
        /// 搜尋結束時間(GMT+8)
        /// </summary>
        public string end_time { get; set; }
    }
}
