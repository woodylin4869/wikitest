namespace ThirdPartyWallet.Share.Model.Game.SPLUS.Response
{
    public class TransferResponse
    {
        /// <summary>
        /// 交易代碼與Request相同的transaction_id
        /// </summary>
        public string transaction_id { get; set; }
        /// <summary>
        /// 遊戲帳號
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 交易金額
        /// </summary>
        public decimal amount { get; set; }
        /// <summary>
        /// 幣別 (使用ISO 4217標準)
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// 轉帳類型
        /// 1=我司轉移到貴司(存款)
        /// 2=貴司轉移到我司(取款)
        /// </summary>
        public int transfer_type { get; set; }
        /// <summary>
        /// 轉帳前錢包餘額
        /// </summary>
        public decimal before_balance { get; set; }
        /// <summary>
        /// 轉帳後錢包餘額
        /// </summary>
        public decimal after_balance { get; set; }
        /// <summary>
        /// 時區:GMT+8
        /// 時間格式:YYYY:MM:DD HH:MM:SS
        /// 2024-07-01 08:00:00  
        /// </summary>
        public string time { get; set; }
    }
}
