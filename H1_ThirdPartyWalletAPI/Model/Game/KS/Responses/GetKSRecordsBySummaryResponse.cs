using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Response
{

    public class GetKSRecordsBySummaryResponse : GetBetRecordResponesBase
    {
        /// <summary>
        /// 游戏ID
        /// </summary>
        public string cateid { get; set; }

        /// <summary>
        /// 主隊英文名称
        /// </summary>
        public string hometeam { get; set; }

        /// <summary>
        /// 客隊英文名称
        /// </summary>
        public string awayteam { get; set; }

        /// <summary>
        /// 投注项名称
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// 赔率
        /// </summary>
        public string odds { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        public decimal betamount { get; set; }

        /// <summary>
        /// 輸贏金額
        /// </summary>
        public decimal money { get; set; }

        /// <summary>
        /// 结算时间
        /// </summary>
        public DateTime? rewardat { get; set; }

        /// <summary>
        /// 联赛名称
        /// </summary>
        public string league { get; set; }


        /// <summary>
        /// 注單類型=type( Single 电竞单关订单/ Combo 电竞串关订单/  Smart 趣味游戏订单/ Anchor 主播订单/ VisualSport 虚拟电竞订单)
        /// </summary>
        public string type { get; set; }

    }


}
