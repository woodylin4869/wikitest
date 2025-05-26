using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
    /// <summary>
    /// 获取每小时投注汇总
    /// </summary>
    public class GetHandsSummaryHourlyResponse
    {
		public List<Data> data { get; set; }
		public Error error { get; set; }

		public class Data
		{
			/// <summary>
			/// 每小时记录的日期和时间
			/// </summary>
			public long dateTime { get; set; }
            /// <summary>
            /// 游戏投注总计数
            /// </summary>
            public int totalHands { get; set; }
            /// <summary>
            /// 记录中玩家使用的货币
            /// </summary>
            public string currency { get; set; }
            /// <summary>
            /// 总投注额
            /// </summary>
            public decimal totalBetAmount { get; set; }
            /// <summary>
            /// 派彩总金额
            /// </summary>
            public decimal totalWinAmount { get; set; }
            /// <summary>
            /// 玩家的总输赢
            /// </summary>
            public decimal totalPlayerWinLossAmount { get; set; }
            /// <summary>
            /// 公司的总输赢
            /// </summary>
            public decimal totalCompanyWinLossAmount { get; set; }
            /// <summary>
            /// 交易类别：
            /// 1: 现金
            /// 2: 红利
            /// 3: 免费游戏
            /// </summary>
            public int transactionType { get; set; }
            /// <summary>
            /// 消除的普通旋转总数
            /// </summary>
            public int totalCollapseSpinCount { get; set; }
            /// <summary>
            /// 消除的免费旋转总数
            /// </summary>
            public int totalCollapseFreeSpinCount { get; set; }
        }


		public class Error
		{
			public string code { get; set; }
			public string message { get; set; }
			public string traceId { get; set; }
		}
	}
}