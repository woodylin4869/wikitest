namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 取出點數
    /// </summary>
    public class WithdrawResponse
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
            /// <summary>
            /// 交易惟一識別碼
            /// </summary>
            public string TransactionID { get; set; }
            /// <summary>
            /// yyyy-MM-dd HH:mm:ss
            /// </summary>
            public string TransactionTime { get; set; }
            /// <summary>
            /// 會員的唯一識別碼
            /// </summary>
            public string UserId { get; set; }
            /// <summary>
            /// 點數交易序號
            /// </summary>
            public string PointID { get; set; }
            /// <summary>
            /// 存入點數
            /// </summary>
            public decimal Balance { get; set; }
            /// <summary>
            /// 會員當前點數
            /// </summary>
            public decimal CurrentPlayerBalance { get; set; }
        }
    }
}