using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Request
{
    /// <summary>
    /// RG富遊API統一的RequestModel
    /// </summary>
    public class RequestBase
    {
        /// <summary>
        /// 加密過後的postData
        /// </summary>
        public string p { get; set; }

        /// <summary>
        /// appKey
        /// </summary>
        public string ak { get; set; }
    }
}