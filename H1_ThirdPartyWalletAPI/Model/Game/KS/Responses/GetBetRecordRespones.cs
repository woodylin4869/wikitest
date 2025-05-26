using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Response
{

    public class GetBetRecordResponesBase
    {
        /// <summary>
        /// 注单ID
        /// </summary>
        public string orderid { get; set; }

        /// <summary>
        /// 订单状态。详情参见本章节订单状态 None 等待开奖 / Cancel 比赛取消 / Win 赢 /Lose 输 / Revoke 无效订单
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 投注时间（毫秒）
        /// </summary>
        public DateTime createat { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime updateat { get; set; }
    }


}
