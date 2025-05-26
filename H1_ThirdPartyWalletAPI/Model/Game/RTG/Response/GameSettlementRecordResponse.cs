using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Response
{
    /// <summary>
    /// 取得遊戲帳務
    /// </summary>
    public class GameSettlementRecordResponse
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
            public int TotalCount { get; set; }
            public int TotalPage { get; set; }
            public int NowPage { get; set; }
            public List<Record> Record { get; set; }
        }
    }

    public class Record
    {
        public Guid summary_id { get; set; }
        public int game_id { get; set; }
        public long RecordId { get; set; }
        public long JiangHao { get; set; }
        public string UserId { get; set; }
        public decimal Bet { get; set; }
        public decimal WinLose { get; set; }
        public decimal Common { get; set; }
        public decimal Surplus { get; set; }
        public decimal Divided { get; set; }
        public int RecordType { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime SettlementTime { get; set; }
    }
}