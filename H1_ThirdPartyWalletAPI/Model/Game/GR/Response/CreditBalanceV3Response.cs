namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0002-v3 - 平台使用者轉入點數 credit_balance_v3
    /// </summary>
    public class CreditBalanceV3Response : GRResponseBase
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
            /// 現存餘額(顯示到小點第二位)
            /// </summary>
            public decimal balance { get; set; }

            /// <summary>
            /// 單號
            /// </summary>
            public string order_id { get; set; }

            /// <summary>
            /// 轉入點數(顯示到小點第二位)
            /// </summary>
            public decimal credit_amount { get; set; }

            /// <summary>
            /// 點數類型(real 真實貨幣)
            /// </summary>
            public string c_type { get; set; }
        }
    }
}
