using Newtonsoft.Json;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    /// <summary>
    /// 查詢玩家在線列表
    /// </summary>
    public class PlayerOnlineListResponse
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
        /// 以 JSON 表示的 object
        /// </summary>
        public List<PlayerOnlineListResponseDatum> data { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }

        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public class PlayerOnlineListResponseDatum
        {
            /// <summary>
            /// 站台代碼，即代理唯一識別碼 ID
            /// </summary>
            public string webid { get; set; }
            /// <summary>
            /// 玩家的唯一識別碼
            /// </summary>
            public string userid { get; set; }
            /// <summary>
            /// 登入時間
            /// </summary>
            public string logintime { get; set; }
            /// <summary>
            /// (0:離線，1:線上)
            /// </summary>
            public int isonline { get; set; }
        }
    }
}
