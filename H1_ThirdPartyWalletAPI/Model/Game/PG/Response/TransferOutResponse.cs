namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
    /// <summary>
    /// 转出余额
    /// </summary>
    public class TransferOutResponse
    {
		public Data data { get; set; }
        public Error error { get; set; }

        public class Data
        {
            /// <summary>
            /// 特殊交易识别码
            /// </summary>
            public long transactionId { get; set; }
            /// <summary>
            /// 交易前的玩家余额
            /// </summary>
            public decimal balanceAmountBefore { get; set; }
            /// <summary>
            /// 交易后的玩家余额
            /// </summary>
            public decimal balanceAmount { get; set; }
            /// <summary>
            /// 交易金额
            /// </summary>
            public decimal amount { get; set; }
        }

        public class Error
        {
            /// <summary>
            /// 3001 不能空值
            ///	3005 玩家钱包不存在
            ///	3100 转账失败
            ///	3101 转账请求进行中，请重试查看最新状态
            /// </summary>
            public string code { get; set; }
            public string message { get; set; }
            public string traceId { get; set; }
        }
	}
}