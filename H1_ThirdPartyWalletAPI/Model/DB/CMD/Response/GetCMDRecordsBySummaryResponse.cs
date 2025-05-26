using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.CMD.Response
{
    public class GetCMDRecordsBySummaryResponse : CMDRecordPrimaryKey
    {

        /// <summary>
        /// 数据 ID
        /// </summary>
        public long SocTransId { get; set; }

        /// <summary>
        /// 球类标识
        /// </summary>
        public string SportType { get; set; }

        /// <summary>
        /// 玩法类型1 1X2 下注 1 2 1X2 下注 2 CS 波胆 FLG 最先/最后进球 HDP 让球 HFT 半场/全场 OE 单/双 OU 大/小 OUT 优胜冠军 PAR 混合过关 TG 总进球 X 1X2 下注 X 1X 双重机会(DC) 下注 1X 12 双重机会(DC) 下注 12 X2 双重机会(DC) 下注 X2 ETG 准确总进球 HTG 主队准确总进球 ATG 客队准确总进球 HP3 三项让分投注 CNS 零失球
        /// </summary>
        public string TransType { get; set; }

        /// <summary>
        /// 是否投注主队
        /// </summary>
        public bool IsBetHome { get; set; }

        /// <summary>
        /// 投注时的赔率
        /// </summary>
        public decimal Odds { get; set; }
        /// <summary>
        /// 盤口
        /// </summary>
        public string OddsType { get; set; }
        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal BetAmount { get; set; }
        /// <summary>
        /// 输赢金额
        /// </summary>
        public decimal WinAmount { get; set; }

        /// <summary>
        /// 注单结算时间(如赛事结束后又重启赛事的情形,则注单结算时间以最后赛事结束时的注单结算时间为准。)
        /// </summary>
        public new DateTime? StateUpdateTs { get; set; }

        /// <summary>
        /// 联赛 ID
        /// </summary>
        public long LeagueId { get; set; }

        /// <summary>
        /// 联赛 En
        /// </summary>
        public string LeagueEn { get; set; }

        public long HomeTeamId { get; set; }

        public string HomeTeamEn { get; set; }

        public long AwayTeamId { get; set; }

        public string AwayTeamEn { get; set; }
    }
}
