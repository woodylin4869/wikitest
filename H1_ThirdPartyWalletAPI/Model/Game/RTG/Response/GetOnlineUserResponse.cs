using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Response
{
    /// <summary>
    /// 取得遊戲中的會員 
    /// </summary>
    public class GetOnlineUserResponse
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
            public string SystemCode { get; set; }
            public string WebId { get; set; }
            public int TotalCount { get; set; }
            public int TotalPage { get; set; }
            public int NowPage { get; set; }
            public List<string> UserList { get; set; }
        }
    }
}