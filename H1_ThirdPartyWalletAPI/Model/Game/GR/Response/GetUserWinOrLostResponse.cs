namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0010 – 平台取得使用者輸贏金額 get_user_win_or_lost
    /// </summary>
    public class GetUserWinOrLostResponse : GRResponseBase
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
            /// 輸贏結果
            /// </summary>
            public decimal win_lost { get; set; }

            /// <summary>
            /// 遊戲代碼(回傳 0 代表所有遊戲, 其他請參照 遊戲代碼對應表)
            /// </summary>
            public int game_type { get; set; }

            /// <summary>
            /// 開始時間
            /// </summary>
            public string start_time { get; set; }

            /// <summary>
            /// 開始時間
            /// </summary>
            public string end_time { get; set; }

            /// <summary>
            /// 點數類型(real 真實貨幣)
            /// </summary>
            public string c_type { get; set; }
        }
    }
}
