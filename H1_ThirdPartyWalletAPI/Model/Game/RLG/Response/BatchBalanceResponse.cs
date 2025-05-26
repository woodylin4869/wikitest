using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    /// <summary>
    /// 批次查詢餘額
    /// </summary>
    public class BatchBalanceResponse
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
        /// json
        /// </summary>
        public List<BatchBalanceResponseDatum> data { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// json
        /// </summary>
        public class BatchBalanceResponseDatum
        {
            /// <summary>
            /// 站台代碼，即代理唯一識別碼ID
            /// </summary>
            public string webid { get; set; }
            /// <summary>
            /// 玩家的唯一識別碼
            /// </summary>
            public string userid { get; set; }
            /// <summary>
            /// 會員目前額度(查無資料為空值),
            /// </summary>
            public decimal balance { get; set; }
        }

    }
}
