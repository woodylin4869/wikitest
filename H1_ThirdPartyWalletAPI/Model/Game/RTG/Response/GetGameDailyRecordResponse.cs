using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Response
{
    /// <summary>
    /// 取得遊戲每日統計資訊
    /// </summary>
    public class GetGameDailyRecordResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int MsgID { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; }
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
            public string SystemCode { get; set; }
            public string WebId { get; set; }
            public List<Gamereport> GameReport { get; set; }
        }
    }
    public class Gamereport
    {
        public string UserId { get; set; }
        /// <summary>
        /// 總帳務筆數
        /// </summary>
        public int PlayCount { get; set; }
        /// <summary>
        /// 總下注
        /// </summary>
        public decimal Bet { get; set; }
        /// <summary>
        /// 總輸贏
        /// </summary>
        public decimal WinLose { get; set; }
        /// <summary>
        /// 抽水、總公點
        /// </summary>
        public decimal Common { get; set; }
        /// <summary>
        /// 總分潤
        /// </summary>
        public decimal Divided { get; set; }
        /// <summary>
        /// 遊戲ID
        /// </summary>
        public int game_id { get; set; }
        /// <summary>
        /// 報表日期
        /// </summary>
        public DateTime report_datetime { get; set; }
    }
}