using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Response
{
    public class GetAgentSummaryBetRecordsResponse : ResponseBaseModel<MessageResult>
    {
        public List<AgentSummaryBetRecord> Rows { get; set; }
    }

    public class AgentSummaryBetRecord
    {
        /// <summary>
        /// 代理帳號
        /// </summary>
        public string agent { get; set; }
        /// <summary>
        /// 下注數量
        /// </summary>
        public string bet_count { get; set; }
        /// <summary>
        /// 下注金額
        /// </summary>
        public decimal bet_amount { get; set; }
        /// <summary>
        /// 遊戲贏分(未扣除手續費)
        /// </summary>
        public decimal payout_amount { get; set; }
        /// <summary>
        /// 有效金額
        /// </summary>
        public decimal valid_amount { get; set; }
        /// <summary>
        /// 手續費
        /// </summary>
        public int fee_amount { get; set; }
        /// <summary>
        /// 彩金金額
        /// </summary>
        public int jp_amount { get; set; }
    }
}