using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 取得點數不為 0 的會員帳戶資訊(已離開遊戲)
    /// </summary>
    public class GetUnwithdrawnResponse
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
            /// 幣別代碼
            /// </summary>
            public string Currency { get; set; }
            /// <summary>
            /// 會員尚餘點數
            /// </summary>
            public decimal Balance { get; set; }
        }
    }
}