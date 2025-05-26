namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Response
{
    /// <summary>
    /// 取得會員下注內容統計
    /// </summary>
    public class GetApiReportUrlResponse : BaseResponse
    {
        /// <summary>
        /// Data object
        /// </summary>
        public DataInfo Data { get; set; }

        /// <summary>
        /// 參數 data 裡的欄位資料
        /// </summary>
        public class DataInfo
        {
            /// <summary>
            /// 代理帳號
            /// </summary>
            public string Agent { get; set; }

            /// <summary>
            /// xg-casino only
            /// </summary>
            public string GameType { get; set; }

            /// <summary>
            /// 下注總額
            /// </summary>
            public decimal ActualBettingAmount { get; set; }

            /// <summary>
            /// 有效下注總額
            /// </summary>
            public decimal TotalBetAmount { get; set; }

            /// <summary>
            /// 會員結果總額(總輸贏 + 總退水)
            /// </summary>
            public decimal TotalPayoff { get; set; }

            /// <summary>
            /// 注單量(By wager)
            /// </summary>
            public int WagersCount { get; set; }

            /// <summary>
            /// 有單為 1, 無單為 3
            /// </summary>
            public int Status { get; set; }

            /// <summary>
            /// 獎金，已棄用
            /// </summary>
            //public decimal TotalJackPot { get; set; }
        }
    }
}
