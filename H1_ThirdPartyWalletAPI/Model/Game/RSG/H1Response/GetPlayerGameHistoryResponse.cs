using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 取得遊戲詳細資訊
    /// </summary>
    public class GetPlayerGameHistoryResponse
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
            public List<GameDetail> GameDetail { get; set; }
        }

        public class GameDetail
        {
            /// <summary>
            /// </summary>
            public int RowId { get; set; }
            /// <summary>
            /// </summary>
            public long SequenNumber { get; set; }
            /// <summary>
            /// </summary>
            public string SessionNo { get; set; }
            /// <summary>
            /// 會員惟一識別碼
            /// </summary>
            public string PlayerId { get; set; }

            /// <summary>
            /// web id
            /// </summary>
            public string WebId { get; set; }
            /// <summary>
            /// 遊戲代碼(請參照代碼表)
            /// </summary>
            public int GameID { get; set; }
            /// <summary>
            /// 會計倍率
            /// </summary>
            public decimal AccDenom { get; set; }
            /// <summary>
            /// 遊戲倍率
            /// </summary>
            public decimal PlayDenom { get; set; }
            /// <summary>
            /// 下注(小數點兩位)
            /// </summary>
            public decimal BetAmt { get; set; }
            /// <summary>
            /// 贏分(小數點兩位)
            /// </summary>
            public decimal WinAmt { get; set; }
            /// <summary>
            /// 彩金(小數點兩位)
            /// </summary>
            public decimal JackpotAccumulateAmt { get; set; }
            /// <summary>
            /// 比倍贏分(小數點兩位)
            /// </summary>
            public decimal GambleWinAmt { get; set; }
            /// <summary>
            /// 彩金贏分(小數點兩位)
            /// </summary>
            public decimal JackpotWinAmt { get; set; }

            /// <summary>
            /// </summary>
            public decimal PrepayAmt { get; set; }
            /// <summary>
            /// 起始餘額
            /// </summary>
            public decimal BeforeCredit { get; set; }
            /// <summary>
            /// 結束餘額
            /// </summary>
            public decimal AfterCredit { get; set; }
            /// <summary>
            /// 匯率
            /// </summary>
            public decimal CurrencyRate { get; set; }
            /// <summary>
            /// 遊戲時間
            /// </summary>
            public string PlayTime { get; set; }
        }
    }
}