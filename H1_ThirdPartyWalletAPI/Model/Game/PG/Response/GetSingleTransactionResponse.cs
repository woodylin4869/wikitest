namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
    /// <summary>
    /// 获取单个交易记录
    /// </summary>
    public class GetSingleTransactionResponse
    {
		public Data data { get; set; }
        public Error error { get; set; }

        public class Data
        {
            /// <summary>
            /// 特殊交易识别码
            /// </summary>
            public string transactionId { get; set; }
            /// <summary>
            /// 玩家帐号
            /// </summary>
            public string playerName { get; set; }
            /// <summary>
            /// 玩家选择的币种
            /// </summary>
            public string currencyCode { get; set; }
            /// <summary>
            /// 交易类型:
            ///	100: TransferInCash
            ///	200: TransferOutCash
            /// </summary>
            public string transactionType { get; set; }
            /// <summary>
            /// 交易金额
            /// </summary>
            public string transactionAmount { get; set; }
            /// <summary>
            /// 交易前的钱包余额
            /// </summary>
            public string transactionFrom { get; set; }
            /// <summary>
            /// 交易后的钱包余额
            /// </summary>
            public string transactionTo { get; set; }
            /// <summary>
            /// 交易的日期和时间（Unix 时间戳，以毫秒为单位）
            /// </summary>
            public string transactionDateTime { get; set; }
        }

        public class Error
        {
            /// <summary>
            ///	3040 交易不存在
            /// </summary>
            public string code { get; set; }
            public string message { get; set; }
            public string traceId { get; set; }
        }
	}
}