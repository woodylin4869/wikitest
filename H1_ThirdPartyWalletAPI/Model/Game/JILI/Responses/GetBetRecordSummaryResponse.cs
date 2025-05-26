namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses
{
    public class GetBetRecordSummaryResponse
    {
        
            public int ErrorCode { get; set; }
            public string Message { get; set; }
            public Datum[] Data { get; set; }
        

        public class Datum
        {
            /// <summary>
            /// 投注總金額
            /// </summary>
            public decimal BetAmount { get; set; }
            /// <summary>
            /// 派彩總金額
            /// </summary>
            public decimal PayoffAmount { get; set; }
            /// <summary>
            /// 有效下注
            /// </summary>
            public decimal Turnover { get; set; }
            public decimal Preserve { get; set; }
            /// <summary>
            /// 注單數量
            /// </summary>
            public int WagersCount { get; set; }
        }

    }
}
