namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0013 – 平台取得使用者登入 (sid) get_sid_by_account
    /// </summary>
    public class GetSidByAccountResponse : GRResponseBase
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
            /// 唯一身份編號
            /// </summary>
            public string sid { get; set; }

            /// <summary>
            /// 到期時間
            /// </summary>
            public string expire_time { get; set; }

            /// <summary>
            /// 遊戲網址
            /// </summary>
            public string game_url { get; set; }
        }
    }
}
