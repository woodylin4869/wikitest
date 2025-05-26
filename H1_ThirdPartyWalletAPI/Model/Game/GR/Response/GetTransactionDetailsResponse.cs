namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0008 – 平台取得交易詳細記錄 get_transaction_details
    /// </summary>
    public class GetTransactionDetailsResponse : GRResponseBase
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
            /// 交易明細陣列
            /// </summary>
            public Transaction_Details[] transaction_details { get; set; }

            /// <summary>
            /// 目前所在分頁
            /// </summary>
            public int page_index { get; set; }

            /// <summary>
            /// 每頁筆數
            /// </summary>
            public int page_size { get; set; }

            /// <summary>
            /// 全部筆數
            /// </summary>
            public int total_pages { get; set; }

            /// <summary>
            /// 全部頁數
            /// </summary>
            public int total_elements { get; set; }
        }

        /// <summary>
        /// 交易明細陣列
        /// </summary>
        public class Transaction_Details
        {
            /// <summary>
            /// 單號
            /// </summary>
            public string order_id { get; set; }

            /// <summary>
            /// 交易金額
            /// </summary>
            public decimal point { get; set; }

            /// <summary>
            /// 玩家目前餘額
            /// </summary>
            public decimal balance { get; set; }

            /// <summary>
            /// 備註
            /// </summary>
            public string memo { get; set; }

            /// <summary>
            /// 創建時間
            /// </summary>
            public string create_time { get; set; }

            /// <summary>
            /// 點數類型(real 真實貨幣)
            /// </summary>
            public string c_type { get; set; }
        }
    }
}
