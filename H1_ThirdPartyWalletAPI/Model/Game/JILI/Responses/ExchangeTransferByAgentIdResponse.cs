namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses
{
    public class ExchangeTransferByAgentIdResponse
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public ResData Data { get; set; }

        public class ResData
        {
            public string TransactionId { get; set; }
            /// <summary>
            /// 轉帳前金額(遊戲幣)
            /// </summary>
            public decimal CoinBefore { get; set; }
            /// <summary>
            /// 轉帳後金額(遊戲幣)
            /// </summary>
            public decimal CoinAfter { get; set; }
            /// <summary>
            /// 轉帳前金額(指定貨幣)
            /// </summary>
            public decimal CurrencyBefore { get; set; }
            /// <summary>
            /// 轉帳後金額(指定貨幣)
            /// </summary>
            public decimal CurrencyAfter { get; set; }
            /// <summary>
            /// 狀態:
            ///1: 成功
            ///2: 失敗
            /// </summary>
            public int Status { get; set; }
        }
    }
}
