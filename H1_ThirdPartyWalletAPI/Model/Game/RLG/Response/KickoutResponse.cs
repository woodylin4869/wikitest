using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    /// <summary>
    /// 剔除玩家
    /// </summary>
    public class KickoutResponse
    {
        /// <summary>
        /// 000000 即為成功，其它代碼皆為失敗
        /// </summary>
        public int errorcode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string errormessage { get; set; }
        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public KickoutResponseData data { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }


        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public class KickoutResponseData
        {
            /// <summary>
            /// 系統代碼
            /// </summary>
            public string systemcode { get; set; }
            /// <summary>
            /// 站台代碼，即代理唯一識別碼 ID
            /// </summary>
            public string webid { get; set; }
            /// <summary>
            /// 玩家的唯一識別碼
            /// </summary>
            public string userid { get; set; }

            /// <summary>
            /// (0:失敗，1:成功)
            /// </summary>
            public int iskickout { get; set; }
        }

    



    }
}
