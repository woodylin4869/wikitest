namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
    /// <summary>
    /// 查看玩家状态
    /// </summary>
    public class CheckResponse
    {
		public Data data { get; set; }
        public Error error { get; set; }

        public class Data
        {
            /// <summary>
            /// 玩家帐号
            /// </summary>
            public string player_name { get; set; }
            /// <summary>
            /// 请求状态
            ///	0: 失败
            ///	1: 成功
            ///	3: 已冻结
            /// </summary>
            public string status { get; set; }
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