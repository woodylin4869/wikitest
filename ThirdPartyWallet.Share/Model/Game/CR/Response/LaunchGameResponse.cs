namespace ThirdPartyWallet.Share.Model.Game.CR.Response
{
    public class LaunchGameResponse : ApiResponseBase
    {
        /// <summary>
        /// 會員名稱
        /// </summary>
        public string memname { get; set; }

        /// <summary>
        /// 會員編號
        /// </summary>
        public string memid { get; set; }

        /// <summary>
        /// 會員登入系統所使用的授權碼
        /// </summary>
        public string memToken { get; set; }

        /// <summary>
        /// 提供會員登入的連結
        /// </summary>
        public string launchgameurl { get; set; }
    }


}