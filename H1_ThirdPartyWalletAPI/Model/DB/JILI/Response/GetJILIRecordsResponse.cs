using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.JILI.Response
{
    public class GetJILIRecordsResponse: JILIRecordPrimaryKey
    {
        /// <summary>
        /// 遊戲的唯一識別值
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// 投注金額
        /// </summary>
        public decimal BetAmount { get; set; }
        /// <summary>
        /// 派彩時間
        /// </summary>
        public DateTime PayoffTime { get; set; }
        /// <summary>
        /// 派彩金額
        /// </summary>
        public decimal PayoffAmount { get; set; }
        /// <summary>
        /// 對帳時間
        /// </summary>
        public DateTime SettlementTime { get; set; }
        /// <summary>
        /// 有效投注金額
        /// </summary>
        public decimal Turnover { get; set; }
    }
}
