namespace ThirdPartyWallet.Share.Model.Game.CR.Response
{
    public class CreateMemberResponse : ApiResponseBase
    {
        public Userdata userdata { get; set; }
    }

    public class Userdata
    {
        /// <summary>
        /// 會員的餘額
        /// </summary>
        public string user_gold { get; set; }

        /// <summary>
        /// 會員名稱(支援API函數3.2) 
        /// </summary>
        public string user_username { get; set; }

        /// <summary>
        /// 會員是否被啟用
        /// </summary>
        public string user_enable { get; set; }

        /// <summary>
        /// 幣別 詳見4.7
        /// </summary>
        public string user_currency { get; set; }
    }

}