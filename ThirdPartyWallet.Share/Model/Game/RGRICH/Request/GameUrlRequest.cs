using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Request
{
    public class GameUrlRequest : DataRequestBase
    {
        /// <summary>
        /// 玩家用戶名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 平台ID RG: 2
        /// </summary>
        public int PlatId { get; set; } = 2;

        /// <summary>
        /// 遊戲代碼
        /// 3001、3002、3006、3007、3009、3005、6001、6002、6003、3010
        /// </summary>
        public string GameCode { get; set; }

        /// <summary>
        /// 裝置
        /// </summary>
        // public string Device { get; set; }
    }
}