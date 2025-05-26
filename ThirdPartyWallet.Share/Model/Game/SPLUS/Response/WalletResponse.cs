namespace ThirdPartyWallet.Share.Model.Game.SPLUS.Response
{
    public class WalletResponse
    {
        /// <summary>
        /// 會員帳號
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 餘額
        /// </summary>
        public decimal balance { get; set; }
        /// <summary>
        /// 幣別
        /// </summary>
        public string currency { get; set; }
    }
}
