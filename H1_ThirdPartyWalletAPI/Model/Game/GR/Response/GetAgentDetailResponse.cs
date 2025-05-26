namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0020 – 平台取得代理額度 get_agent_detail
    /// </summary>
    public class GetAgentDetailResponse : GRResponseBase
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
            /// 代理的帳號
            /// </summary>
            public string account { get; set; }

            /// <summary>
            /// 開分額度
            /// </summary>
            public decimal credits { get; set; }

            /// <summary>
            /// 累積開分額度
            /// </summary>
            public int total_credits { get; set; }

            /// <summary>
            /// 額度開關狀態
            /// </summary>
            public int credit_switch { get; set; }
        }
    }
}