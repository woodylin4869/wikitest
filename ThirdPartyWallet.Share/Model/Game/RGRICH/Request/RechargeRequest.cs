using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Request
{
    public class RechargeRequest : DataRequestBase
    {
        /// <summary>
        /// 玩家用戶名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 充值金額（正數）
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 流水號（站點生成）
        /// </summary>
        public string FlowNumber { get; set; }
    }
}