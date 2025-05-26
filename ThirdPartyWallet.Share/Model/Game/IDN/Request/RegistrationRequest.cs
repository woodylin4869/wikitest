namespace ThirdPartyWallet.Share.Model.Game.IDN.Request
{
    public class RegistrationRequest : DataRequestBase
    {
        /// <summary>
        /// 玩家用戶名
        /// </summary>
        public string username { get; set; }

        /// <summary>
        /// user password
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// user password confirmatiion
        /// </summary>
        public string password_confirmation { get; set; }

        /// <summary>
        /// 姓名或暱稱
        /// </summary>
        public string fullname { get; set; }

        /// <summary>
        /// currency ID that has been given to the client.
        /// </summary>
        public int currency { get; set; }

        /// <summary>
        /// signup ip of user.
        /// </summary>
        public string signup_ip { get; set; }

        /// <summary>
        /// whitelabel ID that has been given to the client.
        /// </summary>
        public int whitelabel_id { get; set; }

        /// <summary>
        /// mobile = 1, if not mobile = 0.
        /// </summary>
        public int is_mobile { get; set; }

        /// <summary>
        /// if you have registration token.
        /// </summary>
        public string reg_token { get; set; }
    }


}