using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.PME.Response
{
    public class GetPMERecordsBySummaryResponse : PMERecordPrimaryKey
    {
        /// <summary>
        /// 游戏ID
        /// </summary>
        public long game_id { get; set; }

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
        public string odd_name { get; set; }

        /// <summary>
        /// 赔率
        /// </summary>
        public string odd { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        public decimal bet_amount { get; set; }

        /// <summary>
        /// 中奖金额
        /// </summary>
        public decimal win_amount { get; set; }

        /// <summary>
        /// 结算时间
        /// </summary>
        public DateTime? settle_time { get; set; }

        /// <summary>
        /// 联赛名称
        /// </summary>
        public string tournament { get; set; }

    }
}
