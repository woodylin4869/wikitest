using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Response
{
    public class tournamentWinsResponse
    {
        /// <summary>
        /// 期數
        /// </summary>
        public int tournamentPeriodId { get; set; }
        /// <summary>
        /// 活動號
        /// </summary>
        public int tournamentId { get; set; }
        public string tournamentName { get; set; }
        /// <summary>
        /// 活動開始時間
        /// </summary>
        public DateTime periodStartDate { get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime periodEndDate { get; set; }
        /// <summary>
        /// 派彩時間
        /// </summary>
        public DateTime creditDate { get; set; }
        public string currency { get; set; }
        /// <summary>
        /// 會員id
        /// </summary>
        public string playerId { get; set; }
        public string aliasName { get; set; }
        public int rank { get; set; }
        public decimal winAmount { get; set; }
        public string winType { get; set; }
        public string prizeType { get; set; }
    }
}
