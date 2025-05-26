using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 取得遊戲每日統計資訊
    /// </summary>
    public class GetGameDailyReportResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// API 呼叫回傳的 JSON 格式的 object / object array
        /// </summary>
        public DataInfo Data { get; set; }

        public class DataInfo
        {
            public List<GameReport> GameReport { get; set; }
        }

        public class GameReport
        {
            /// <summary>
            /// 幣別代碼
            /// </summary>
            public string Currency { get; set; }
            /// <summary>
            /// 站台代碼
            /// </summary>
            public string WebId { get; set; }
            /// <summary>
            /// 會員惟一識別碼
            /// </summary>
            public string UserId { get; set; }
            /// <summary>
            /// 遊戲代碼(請參照代碼表)
            /// </summary>
            public int GameId { get; set; }
            /// <summary>
            /// 統計時間(分鐘)
            /// </summary>
            public string TimeMinute { get; set; }
            /// <summary>
            /// 下注(小數點兩位)
            /// </summary>
            public decimal BetSum { get; set; }
            /// <summary>
            /// 贏分(小數點兩位)
            /// </summary>
            public decimal WinSum { get; set; }
            /// <summary>
            /// 彩金(小數點兩位)
            /// </summary>
            public decimal JackpotWinSum { get; set; }
            /// <summary>
            /// 總輸贏(小數點兩位)
            /// </summary>
            public decimal NetWinSum { get; set; }
            /// <summary>
            /// 該分鐘起始的遊戲紀錄編號
            /// </summary>
            public long SequenNumber { get; set; }
            /// <summary>
            /// 筆數
            /// </summary>
            public int RecordCount { get; set; }
            /// <summary>
            /// Jackpot 貢獻值(小數點五位)
            /// </summary>
            public decimal JackpotContributionSum { get; set; }
        }
    }
}