namespace ThirdPartyWallet.Share.Model.Game.VA.Response
{
    public class GetBalanceResponse
    {
        /// <summary>
        /// 渠道編號
        /// </summary>
        public int ChannelId { get; set; }

        /// <summary>
        /// 會員帳號
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 幣別 (請洽詢我方商務)
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        public decimal Balance { get; set; }
    }
}
