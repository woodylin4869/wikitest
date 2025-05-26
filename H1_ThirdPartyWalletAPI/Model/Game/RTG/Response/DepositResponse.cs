using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Response
{
    /// <summary>
    /// 存入點數 
    /// </summary>
    public class DepositResponse
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
            /// <summary>
            /// 玩家的唯一識別碼
            /// </summary>
            public string UserId { get; set; }
            /// <summary>
            /// 交易惟一識別碼
            /// </summary>
            public string TransactionID { get; set; }
            /// <summary>
            /// 玩家提款前剩餘額度
            /// </summary>
            public decimal BeforeBalance { get; set; }
            /// <summary>
            /// 提款金額
            /// </summary>
            public decimal Balance { get; set; }
            /// <summary>
            /// 玩家提款後剩餘額度
            /// </summary>
            public decimal AfterBalance { get; set; }
            /// <summary>
            /// 交易類型 1:存款 2:提款
            /// </summary>
            public int TransactionType { get; set; }
            /// <summary>
            /// 交易時間 格式範例：2021-02-01 00:00:00
            /// </summary>
            public DateTime TransactionDate { get; set; }
        }
    }
}