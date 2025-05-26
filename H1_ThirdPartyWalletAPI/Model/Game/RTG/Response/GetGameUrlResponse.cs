namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Response
{
    /// <summary>
    /// 取得遊戲網址(進入遊戲) 
    /// </summary>
    public class GetGameUrlResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int MsgID { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// API 呼叫回傳的 JSON 格式的 object / object array
        /// </summary>
        public DataInfo Data { get; set; }

        public class DataInfo
        {
            /// <summary>
            /// 會員的唯一識別碼
            /// </summary>
            public string UserId { get; set; }
            /// <summary>
            /// 大廳連結網址
            /// </summary>
            public string GameUrl { get; set; }

        }
    }
}