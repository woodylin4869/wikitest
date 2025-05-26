namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Response
{
    /// <summary>
    /// 查詢點數交易結果
    /// </summary>
    public class SingleTransactionResponse
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
            public string UserId { get; set; }
            public string TransactionID { get; set; }
            public decimal BeforeBalance { get; set; }
            public decimal Balance { get; set; }
            public decimal AfterBalance { get; set; }
            public int TransactionType { get; set; }
            public string TransactionDate { get; set; }
        }
    }
}