namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    /// <summary>
    /// 存款
    /// </summary>
    public class DepositResponse
    {
        /// <summary>
        /// 操作批次號
        /// </summary>
        public string SerialKey { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 訊息說明
        /// </summary>
        public string Message { get; set; }
     
        public Data Data { get; set; }
    }

    public class Data
    {
        /// <summary>
        /// 用戶可用餘額
        /// </summary>
        public float BetAmount { get; set; }
        /// <summary>
        /// 用戶未結算餘額
        /// </summary>
        public float Outstanding { get; set; }
        /// <summary>
        /// 廠商系統轉帳流水號
        /// </summary>
        public int PaymentId { get; set; }
    }
}

