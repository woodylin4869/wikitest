using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Request
{
    public class KickUserRequest : DataRequestBase
    {
        /// <summary>
        /// 玩家用戶名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 踢線時間(分鐘) 預設5分鐘
        /// </summary>
        public int Min { get; set; } = 0;
    }
}