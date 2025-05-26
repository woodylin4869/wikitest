namespace ThirdPartyWallet.Share.Model.Game.CR.Request
{
    public class CreateMemberRequest : DataRequestBase
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
        /// 幣別 詳見4.7 
        /// </summary>

        public string currency { get; set; }
    }
}