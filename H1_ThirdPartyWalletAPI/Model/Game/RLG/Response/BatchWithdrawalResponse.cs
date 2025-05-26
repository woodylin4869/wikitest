using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    /// <summary>
    /// 批次會員提款
    /// </summary>
    public class BatchWithdrawalResponse
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
        /// 會員資料
        /// </summary>
        public List<BatchWithdrawalResponseDatum> data { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// 會員資料
        /// </summary>
        public class BatchWithdrawalResponseDatum
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
            ///  會員目前額度(查無資料為空值)
            /// </summary>
            public int balance { get; set; }
            /// <summary>
            /// 提款結果(0:提款失敗，1:提款成功)
            /// </summary>
            public int status { get; set; }
            /// <summary>
            /// 交易編號
            /// </summary>
            public string transferno { get; set; }
        }

        

    }
}
