namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    /// <summary>
    /// 取得 URL Token
    /// </summary>
    public class GetURLTokenResponse
    {
        /// <summary>
        /// 000000 即為成功，其它代碼皆為失敗，可參詳 ErrorMessage 欄位內容
        /// </summary>
        public string errorcode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string errormessage { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public DataInfo data { get; set; }

        public class DataInfo
        {
            /// <summary>
            /// token key
            /// </summary>
            public string urltoken { get; set; }
            /// <summary>
            /// 遊戲平台網址
            /// </summary>
            public string url { get; set; }
        }
    }
}
