using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Request
{
    public class RechargeOrWithdrawRecordRequest : DataRequestBase
    {
        /// <summary>
        /// 流水號（站點生成）(玩家提現 or 玩家充值的 FlowNumber)
        /// </summary>
        public string FlowNumber { get; set; }
    }
}