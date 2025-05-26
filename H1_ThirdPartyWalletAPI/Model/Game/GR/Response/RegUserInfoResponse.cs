namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0004 – 平台註冊使用者 reg_user_info
    /// </summary>
    public class RegUserInfoResponse : GRResponseBase
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
            /// 幣別
            /// </summary>
            public string currency_type { get; set; }

            /// <summary>
            /// 創建時間(台灣時間 ex : 2019-11-15T16:04:05+08:00)
            /// </summary>
            public string create_time { get; set; }
        }
    }
}
