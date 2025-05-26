using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Request
{
    public class BetDetailUrlRequest : DataRequestBase
    {
        /// <summary>
        /// 玩家用戶名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 注單編號
        /// </summary>
        public string BetNo { get; set; }
    }
}