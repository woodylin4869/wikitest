namespace ThirdPartyWallet.Share.Model.Game.CR.Request
{
    public class LaunchGameRequest : DataRequestBase
    {
        /// <summary>
        /// 申請會員名稱
        /// </summary>
        public string memname { get; set; }

        /// <summary>
        /// 會員使用的密碼 
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// IP(這邊請提供會員IP) 
        /// </summary>
        public string remoteip { get; set; }

        /// <summary>
        /// 裝置PC or MOBILE 
        /// </summary>
        public string machine { get; set; }

        /// <summary>
        /// 語系 詳見4.4
        /// </summary>
        public string langx { get; set; }

        /// <summary>
        /// 是否使用https 通訊協定
        /// </summary>
        public string isSSL { get; set; }

        /// <summary>
        /// 幣別 詳見4.7
        /// </summary>
        public string currency { get; set; }
    }

}