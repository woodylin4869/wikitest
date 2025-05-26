namespace ThirdPartyWallet.Share.Model.Game.VA.Request
{
    public class CreateRequest
    {
        /// <summary>
        /// 渠道編號
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// 會員帳號 (允許字元: a-z A-Z 0-9 _-)
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 幣別 (請洽詢我方商務)
        /// </summary>
        public string Currency { get; set; }
    }
}
