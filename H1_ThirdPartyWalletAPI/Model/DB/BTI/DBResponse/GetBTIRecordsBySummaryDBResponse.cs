namespace H1_ThirdPartyWalletAPI.Model.DB.BTI.DBResponse
{
    public class GetBTIRecordsBySummaryDBResponse : BTIRecordPrimaryKeyUpdateDateNull
    {
        /// <summary>
        /// 下注类别 ID
        /// 1 Single bets 单场投注
        /// 2 Combo bets 组合投注
        /// 3 System bet 系统投注
        /// 5 QA Bet QA 投注
        /// 6 Exact Score 准确比分
        /// 7 QA Bet QA 投注
        /// 13 System bet 系统投注
        /// </summary>
        //public int BetTypeId { get; set; }

        /// <summary>
        /// 下注类别
        /// 1 Single bets 单场投注
        /// 2 Combo bets 组合投注
        /// 3 System bet 系统投注
        /// 5 QA Bet QA 投注
        /// 6 Exact Score 准确比分
        /// 7 QA Bet QA 投注
        /// 13 System bet 系统投注
        /// 
        /// 解释：QA 投注是问答形式的注单 常见的种类是 outright 赌冠军因为赌冠军的类型
        /// 是"请问以下队伍谁会夺冠" 答案会是一个字串:"A 队"所以我们把这种注单叫 QA 投注
        /// </summary>
        public string BetTypeName { get; set; }

        /// <summary>
        /// 玩家看到的赔率
        /// </summary>
        public string OddsInUserStyle { get; set; }

        /// <summary>
        /// 玩家使用的盘口 比如 European 欧洲盘 / Malay 马来盘 / Hongkong 香港盘 / Indo 印尼盘 / American 美盘
        /// </summary>
        public string OddsStyleOfUser { get; set; }

        /// <summary>
        /// 净利（扣除了下注额）
        /// </summary>
        public decimal PL { get; set; }

        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal TotalStake { get; set; }

        /// <summary>
        /// 正式下注额 （如果是提前兑现是永远是0）
        /// </summary>
        public decimal ValidStake { get; set; }

        /// <summary>
        /// BranchName – 体育类型名
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// BranchID - 体育类别 ID 
        /// </summary>
        public int? BranchID { get; set; }

        /// <summary>
        /// LeagueName 联赛名
        /// </summary>
        public string LeagueName { get; set; }

        /// <summary>
        /// HomeTeam 主队名
        /// </summary>
        public string HomeTeam { get; set; }

        /// <summary>
        /// AwayTeam 客队名
        /// </summary>
        public string AwayTeam { get; set; }

        /// <summary>
        /// YourBet – 玩家下注选项, 如果是百家乐，请看附录 4.2.1
        /// </summary>
        public string YourBet { get; set; }
    }
}
