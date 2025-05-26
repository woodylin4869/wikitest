using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 取得遊戲詳細資訊
    /// </summary>
    public class GetPlayerFishGameMinuteSummaryResponse
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
            public string TimeMinute { get; set; }
            /// <summary>
            /// 下注(小數點兩位)
            /// </summary>
            public decimal BetAmtSum { get; set; }
            /// <summary>
            /// 贏分(小數點兩位)
            /// </summary>
            public decimal WinAmtSum { get; set; }
            /// <summary>
            /// 彩金(小數點兩位)
            /// </summary>
            public decimal JackpotWinAmtSum { get; set; }
            /// <summary>
            /// 比倍贏分(小數點兩位)
            /// </summary>
            public decimal DataCount { get; set; }
        }
    }
}