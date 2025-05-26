using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Response
{
    public class GetbalanceResponse
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public int status_code {  get; set; }
        /// <summary>
        /// 餘額
        /// </summary>
        public decimal balance { get; set; }
        /// <summary>
        /// 其他訊息
        /// </summary>
        public string message { get; set; }
    }
}
