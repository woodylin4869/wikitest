namespace ThirdPartyWallet.Share.Model.Game.SPLUS.Request
{
    public class GetlinkRequest
    {
        /// <summary>
        /// 廠商提供的gamecode(唯一值)
        /// </summary>
        public string gamecode {  get; set; }
        /// <summary>
        /// 遊戲帳號
        /// </summary>
        public string account {  get; set; }
        /// <summary>
        /// 語系(預設en-us)
        /// </summary>
        public string lang { get; set; }
        /// <summary>
        /// 語系(預設en-us)
        /// </summary>
        public string betlimit { get; set; }
        /// <summary>
        /// 返回我方網站網址
        /// </summary>
        public string returnurl { get; set; }

    }
}
