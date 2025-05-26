namespace ThirdPartyWallet.Share.Model.Game.VA.Request
{

    public class KickUserRequest
    {
        /// <summary>
        /// 渠道編號
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// 會員帳號 (允許字元: a-z A-Z 0-9 _-)
        /// </summary>
        public string Account { get; set; }
    }
}
