using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 查詢點數交易歷程
    /// </summary>
    public class GetTransactionHistoryResponse
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
            public List<Tranhistory> TranHistory { get; set; }
        }

        public class Tranhistory
        {
            /// <summary>
            /// 交易惟一識別碼
            /// </summary>
            public string TransactionID { get; set; }
            /// <summary>
            /// yyyy-MM-dd HH:mm:ss
            /// </summary>
            public string TransactionTime { get; set; }
            /// <summary>
            /// 點數交易序號
            /// </summary>
            public string PointID { get; set; }
            /// <summary>
            /// 1.存點 2.取點
            /// </summary>
            public int Action { get; set; }
            /// <summary>
            /// 交易點數
            /// </summary>
            public decimal Balance { get; set; }
            /// <summary>
            /// 交易後點數
            /// </summary>
            public decimal AfterBalance { get; set; }
        }
    }
}