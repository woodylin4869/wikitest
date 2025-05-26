namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0018 – 平台檢查是否已有單號存在 check_order_exist_v3
    /// </summary>
    public class CheckOrderExistV3Response : GRResponseBase
    {
        /// <summary>
        /// data object
        /// </summary>
        public DataInfo data { get; set; }

        /// <summary>
        /// 參數 data 裡的欄位資料
        /// </summary>
        public class DataInfo
        {
            /// <summary>
            /// 使用者帳號有包含後綴碼
            /// </summary>
            public string account { get; set; }

            /// <summary>
            /// 單號
            /// </summary>
            public string order_id { get; set; }

            /// <summary>
            /// 0. 訂單不存在
            /// 1. 訂單處理中
            /// 2. 訂單處理成功
            /// 3. 訂單處理失敗
            /// </summary>
            public int order_state { get; set; }
        }
    }
}
