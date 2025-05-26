namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Request
{
    /// <summary>
    /// 單筆交易紀錄查詢
    /// </summary>
    public class SingleTransactionRequest
    {
        /// <summary>
        /// 交易代碼
        /// 交易代碼為自行產生的唯一代碼(不可含特殊字元)，最少4碼，最多30碼
        /// </summary>
        public string transaction_id { get; set; }
    }
}
