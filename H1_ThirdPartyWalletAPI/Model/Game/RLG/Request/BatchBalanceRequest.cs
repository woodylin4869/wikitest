using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Request
{
    /// <summary>
    /// 批次查詢餘額
    /// </summary>
    public class BatchBalanceRequest
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        public string SystemCode { get; set; }
        /// <summary>
        /// 會員資料
        /// </summary>
        public List<BatchBalanceRequestData> Data { get; set; }

        public class BatchBalanceRequestData
        {
            /// <summary>
            /// 站台代碼，即代理唯一識別碼ID
            /// </summary>
            public string WebId { get; set; }
            /// <summary>
            /// 玩家的唯一識別碼
            /// </summary>
            public string UserId { get; set; }
        }
    }

    public class BatchBalancepostdata
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        public string SystemCode { get; set; }

        /// <summary>
        /// 會員資料
        /// </summary>
        public string Data { get; set; }
    }
}
