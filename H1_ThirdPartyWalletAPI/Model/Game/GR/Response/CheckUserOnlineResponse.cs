namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0001 – 平台確認使用者是否在線上 check_user_online 
    /// </summary>
    public class CheckUserOnlineResponse : GRResponseBase
    {
        /// <summary>
        /// data object
        /// </summary>
        public DataInfo data { get; set; }

        /// <summary>
        /// 參數 data 裡的欄位資料
        /// </summary>
        public class DataInfo
        {
            /// <summary>
            /// 使用者帳號有包含後綴碼
            /// </summary>
            public string account { get; set; }

            /// <summary>
            /// 使用者名稱
            /// </summary>
            public string display_name { get; set; }

            /// <summary>
            /// 是否在線上
            /// </summary>
            public bool is_online { get; set; }
        }
    }
}
