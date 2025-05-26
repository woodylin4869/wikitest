namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0009 –平台取得所有在線遊戲有效投注總額 get_user_bet_amount
    /// </summary>
    public class GetUserBetAmountResponse : GRResponseBase
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
            /// 遊戲代碼(回傳 0 代表所有遊戲, 其他請參照 遊戲代碼對應表)
            /// </summary>
            public int game_type { get; set; }

            /// <summary>
            /// 所查詢遊戲的有效投注總額
            /// </summary>
            public int stake_amount { get; set; }

            /// <summary>
            /// 開始時間
            /// </summary>
            public string start_time { get; set; }

            /// <summary>
            /// 結束時間
            /// </summary>
            public string end_time { get; set; }

            /// <summary>
            /// 點數類型(real 真實貨幣)
            /// </summary>
            public string c_type { get; set; }
        }
    }
}
