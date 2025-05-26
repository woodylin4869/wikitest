namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Request
{
    /// <summary>
    /// Deposit
    /// </summary>
    public class DepositRequest
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        public string SystemCode { get; set; }
        /// <summary>
        /// 站台代碼，即代理唯一識別碼 ID
        /// </summary>
        public string WebId { get; set; }
        /// <summary>
        /// 玩家的唯一識別碼
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 存款額度
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 交易編號可空值，空值時系統將自動生成
        /// </summary>
        public string? TransferNo { get; set; }
    }
}
