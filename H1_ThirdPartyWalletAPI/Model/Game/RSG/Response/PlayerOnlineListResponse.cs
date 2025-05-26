using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 取得遊戲中的會員 
    /// </summary>
    public class PlayerOnlineListResponse
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
            /// 總筆數
            /// </summary>
            public int DataCount { get; set; }
            /// <summary>
            /// 每頁筆數
            /// </summary>
            public int PageSize { get; set; }
            /// <summary>
            /// 總頁數
            /// </summary>
            public int PageCount { get; set; }
            /// <summary>
            /// 目前頁數
            /// </summary>
            public int PageNow { get; set; }
            /// <summary>
            /// UserList
            /// </summary>
            public List<UserInfo> UserList { get; set; }
        }

        public class UserInfo
        {
            /// <summary>
            /// 站台代碼
            /// </summary>
            public string WebId { get; set; }
            /// <summary>
            /// 會員惟一識別碼
            /// </summary>
            public string UserId { get; set; }
            /// <summary>
            /// 遊戲代碼
            /// </summary>
            public int GameId { get; set; }
        }

    }
}