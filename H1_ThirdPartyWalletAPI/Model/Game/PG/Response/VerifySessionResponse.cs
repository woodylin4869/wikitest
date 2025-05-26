namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
    /// <summary>
    /// 令牌验证
    /// </summary>
    public class VerifySessionResponse
    {
		public Data data { get; set; }
        public Error error { get; set; }

        public class Data
        {
            /// <summary>
            /// 玩家帐号
            ///	玩家名称不区分大小写
            ///	仅允许使用字母，数字以及"@"、 "-"、 "_"符号
            ///	注: 上限 50 字符
            /// </summary>
            public string player_name { get; set; }
            /// <summary>
            /// 玩家昵称
            /// 注: 上限 50 字符
            /// </summary>
            public string nickname { get; set; }
            /// <summary>玩家选择的币种</summary>
            public string currency { get; set; }
        }

        public class Error
        {
            /// <summary>
            /// 1034 无效请求
            /// 1200 内部服务器错误
            /// </summary>
            public string code { get; set; }
            public string message { get; set; }
            public string traceId { get; set; }
        }
	}
}