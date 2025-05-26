namespace ThirdPartyWallet.Share.Model.Game.CR.Request
{
    public class KickOutMemRequest
    {
        /// <summary>
        /// 申請會員名稱
        /// </summary>
        public string memname { get; set; }
        /// <summary>
        /// 接口請求授權的Token
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// 主機時間(毫秒) 
        /// </summary>
        public long timestamp { get; set; }
    }


}