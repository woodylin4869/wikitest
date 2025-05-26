namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
    /// <summary>
    /// 获取运营商令牌
    /// </summary>
    public class LoginProxyResponse
    {
		public Data data { get; set; }
        public Error error { get; set; }

        public class Data
        {
            /// <summary>运营商令牌</summary>
            public string operator_session { get; set; }
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