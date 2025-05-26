using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 查詢會員線上狀態
    /// </summary>
    public class GetPlayerOnlineStatusResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }
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
            /// 會員惟一識別碼
            /// </summary>
            public string UserId { get; set; }
            public List<Online> OnlineList { get; set; }
        }

        public class Online
        {
            /// <summary>
            /// 幣別代碼
            /// </summary>
            public string Currency { get; set; }
            /// <summary>
            /// 遊戲代碼
            /// </summary>
            public int GameId { get; set; }
        }
    }
}