using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Request
{
    /// <summary>
    /// 批次會員提款
    /// </summary>
    public class BatchWithdrawalRequest
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        public string SystemCode { get; set; }

        public List<BatchWithdrawalRequestData> Data { get; set; }

        public class BatchWithdrawalRequestData
        {
            /// <summary>
            /// 站台代碼，即代理唯一識別碼ID
            /// </summary>
            public string WebId { get; set; }
            /// <summary>
            /// 玩家的唯一識別碼
            /// </summary>
            public string UserId { get; set; }
            /// <summary>
            /// 提款額度
            /// </summary>
            public decimal Balance { get; set; }
            /// <summary>
            /// 交易編號(可空值)，空值時系統將自動生成
            /// </summary>
            public string? TransferNo { get; set; }

        }
    }

    public class BatchWithdrawalpostdata
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        public string SystemCode { get; set; }

        public string Data { get; set; }

    }

}
