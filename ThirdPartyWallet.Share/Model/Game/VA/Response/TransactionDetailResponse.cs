namespace ThirdPartyWallet.Share.Model.Game.VA.Response
{
    public class TransactionDetailResponse
    {
        /// <summary>
        /// 交易代碼
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// 交易狀態 (true:正常、false:異常)
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 渠道編號
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// 會員帳號
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 幣別 (請洽詢我方商務)
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 轉帳金額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 轉帳前錢包餘額
        /// </summary>
        public decimal BalanceBefore { get; set; }

        /// <summary>
        /// 轉帳後錢包餘額
        /// </summary>
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// 轉帳時間 (時區、格式請參考API注意事項)
        /// </summary>
        public DateTime Time { get; set; }
    }
}
