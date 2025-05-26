namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
    /// <summary>
    /// 踢出玩家
    /// </summary>
    public class KickResponse
    {
		public Data data { get; set; }
        public Error error { get; set; }

        public class Data
        {
            /// <summary>
            /// 请求状态
            ///	1: 成功
            ///	0: 失败
            /// </summary>
            public string action_result { get; set; }
        }

        public class Error
        {
            /// <summary>
            ///	1034 无效请求
            ///	1035 行动失败
            ///	1200 内部服务器错误
            ///	1204 无效运营商
            /// </summary>
            public string code { get; set; }
            public string message { get; set; }
            public string traceId { get; set; }
        }
	}
}