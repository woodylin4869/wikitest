using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// H1取得遊戲中的會員 
    /// </summary>
    public class GetOnlineMemberListResponse
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
            /// 會員暱稱
            /// </summary>
            public string UserName { get; set; }
            /// <summary>
            /// 遊戲代碼
            /// </summary>
            public int GameId { get; set; }
            /// <summary>
            /// 遊戲名稱(繁體)
            /// </summary>
            public string GameCName { get; set; }
            /// <summary>
            /// 登入時間
            /// </summary>
            public string LoginTime { get; set; }
        }

    }
}