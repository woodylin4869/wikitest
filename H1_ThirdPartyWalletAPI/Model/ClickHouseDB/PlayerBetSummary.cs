using System;

namespace H1_ThirdPartyWalletAPI.Model.ClickHouseDB
{
    /// <summary>
    /// t_player_summary_5min
    /// </summary>
    public class PlayerSummary
    {
        /// <summary>
        /// 匯總時間
        /// </summary>
        public DateTimeOffset report_time { get; set; }

        // 自定義屬性將 `report_time` 字串轉換成 `DateTime`
        //public DateTime ReportTimeParsed
        //{
        //    get => DateTime.Parse(report_time);
        //}

        /// <summary>
        /// 遊戲館
        /// </summary>
        public string platform { get; set; }

        /// <summary>
        /// 遊戲ID
        /// </summary>
        public string game_id { get; set; }

        /// <summary>
        /// 會員ID
        /// </summary>
        public string club_id { get; set; }

        /// <summary>
        /// 總比數
        /// </summary>
        public long total_count { get; set; }

        /// <summary>
        /// 總下注
        /// </summary>
        public decimal BetAmount { get; set; }

        /// <summary>
        /// 贏分
        /// </summary>
        public decimal WinAmount { get; set; }

        /// <summary>
        /// 淨贏分 NetWinAmount 大於 0
        /// </summary>
        public decimal Win { get; set; }

        /// <summary>
        /// 淨輸分 NetWinAmount 小於 0
        /// </summary>
        public decimal LoseAmount { get; set; }

        /// <summary>
        /// 淨輸贏
        /// </summary>
        public decimal NetWinAmount { get; set; }

        /// <summary>
        /// 彩金
        /// </summary>
        public decimal JackPot { get; set; }

        /// <summary>
        /// 更新時間（可以為空）
        /// </summary>
        public DateTimeOffset? update_datetime { get; set; }


        // 額外屬性將 report_time 轉為 DateTime
        public DateTime ReportTime => report_time.DateTime;

        // 額外屬性將 update_datetime 轉為 DateTime?（若有值）
        public DateTime? UpdateDateTime => update_datetime?.DateTime;
    }



    /// <summary>
    /// t_player_summary_5min
    /// </summary>
    public class PlayerSummaryDay
    {
        /// <summary>
        /// 匯總時間
        /// </summary>
        public DateOnly ReportDate { get; set; }

        /// <summary>
        /// 遊戲館
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// 遊戲ID
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        /// 會員ID
        /// </summary>
        public string ClubId { get; set; }

        /// <summary>
        /// 總比數
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// 總下注
        /// </summary>
        public decimal BetAmount { get; set; }

        /// <summary>
        /// 贏分
        /// </summary>
        public decimal WinAmount { get; set; }

        /// <summary>
        /// 淨贏分 NetWinAmount 大於 0
        /// </summary>
        public decimal Win { get; set; }

        /// <summary>
        /// 淨輸分 NetWinAmount 小於 0
        /// </summary>
        public decimal LoseAmount { get; set; }

        /// <summary>
        /// 淨輸贏
        /// </summary>
        public decimal NetWinAmount { get; set; }

        /// <summary>
        /// 彩金
        /// </summary>
        public decimal JackPot { get; set; }
    }

}
