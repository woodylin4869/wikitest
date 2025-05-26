namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Response
{
    /// <summary>
    /// 取得會員登入連結
    /// </summary>
    public class LoginResponse : BaseResponse
    {
        /// <summary>
        /// Data object
        /// </summary>
        public DataInfo Data { get; set; }

        /// <summary>
        /// 參數 data 裡的欄位資料
        /// </summary>
        public class DataInfo
        {
            /// <summary>
            /// 一次性登入連結(已含 token)
            /// </summary>
            public string LoginUrl { get; set; }

            /// <summary>
            /// 一次性登入連結的 token
            /// </summary>
            public string Token { get; set; }
        }
    }
}
