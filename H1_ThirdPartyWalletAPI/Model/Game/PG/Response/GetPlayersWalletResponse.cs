using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
    public class GetPlayersWalletResponse
    {
		public Data data { get; set; }
        public Error error { get; set; }

        public class Data
        {
            /// <summary>
            /// 记录总数
            /// </summary>
            public int totalCount { get; set; }
            /// <summary>
            /// 可用批次的总数
            /// </summary>
            public int totalPage { get; set; }
            /// <summary>
            /// 玩家钱包详情列表
            /// </summary>
            public List<Result> result { get; set; }
        }

        public class Result
        {
            /// <summary>
            /// 玩家名称
            /// </summary>
            public string playerName { get; set; }
            /// <summary>
            /// 玩家选择的币种
            /// </summary>
            public string currencyCode { get; set; }
            /// <summary>
            /// 玩家钱包余额
            /// </summary>
            public float totalBalance { get; set; }
        }

        public class Error
        {
            /// <summary>
            ///	3001 不能空值
            ///	3005 玩家钱包不存在 
            /// </summary>
            public string code { get; set; }
            public string message { get; set; }
            public string traceId { get; set; }
        }
	}
}