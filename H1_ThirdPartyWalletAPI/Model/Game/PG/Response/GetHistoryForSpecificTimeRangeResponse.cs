using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
	/// <summary>
	/// 获取特定时间内的历史记录
	/// </summary>
	public class GetHistoryForSpecificTimeRangeResponse
    {
		public List<Data> data { get; set; }
		public Error error { get; set; }

		public class Data
		{
            public Guid summary_id { get; set; }
			/// <summary>
			/// 母注单的唯一标识符
			/// </summary>
			public long parentBetId { get; set; }
			/// <summary>
			/// 子投注的唯一标识符 （唯一键值）
			/// </summary>
			public long betId { get; set; }
			/// <summary>
			/// 玩家的唯一标识符
			/// </summary>
			public string playerName { get; set; }
			/// <summary>
			/// 游戏的唯一标识符
			/// </summary>
			public int gameId { get; set; }
			/// <summary>
			/// 投注记录类别：
			/// 1: 真实游戏
			/// </summary>
			public int betType { get; set; }
			/// <summary>
			/// 交易类别：
			///	1: 现金
			///	2: 红利
			///	3: 免费游戏
			/// </summary>
			public int transactionType { get; set; }
			/// <summary>
			/// 投注记录平台（请参考平台以获取更多信息）
			/// </summary>
			public int platform { get; set; }
			/// <summary>
			/// 记录货币
			/// </summary>
			public string currency { get; set; }
			/// <summary>
			/// 玩家的投注额
			/// </summary>
			public decimal betAmount { get; set; }
			/// <summary>
			/// 玩家的所赢金额
			/// </summary>
			public decimal winAmount { get; set; }
			/// <summary>
			/// 玩家的奖池返还率贡献额
			/// </summary>
			public decimal jackpotRtpContributionAmount { get; set; }
			/// <summary>
			/// 玩家的奖池贡献额
			/// </summary>
			public decimal jackpotContributionAmount { get; set; }
			/// <summary>
			/// 玩家的奖池金额
			/// </summary>
			public decimal jackpotWinAmount { get; set; }
			/// <summary>
			/// 玩家交易前的余额
			/// </summary>
			public decimal balanceBefore { get; set; }
			/// <summary>
			/// 玩家交易后的余额
			/// </summary>
			public decimal balanceAfter { get; set; }
			/// <summary>
			/// 投注状态：
			///	1: 非最后一手投注
			///	2：最后一手投注
			///	3：已调整
			/// </summary>
			public int handsStatus { get; set; }
			/// <summary>
			/// 数据更新时间（以毫秒为单位的 Unix 时间戳）
			/// </summary>
			public long rowVersion { get; set; }
			/// <summary>
			/// 当前投注的开始时间（以毫秒为单位的 Unix 时间戳）
			/// </summary>
			public long betTime { get; set; }
			/// <summary>
			/// 当前投注的结束时间（以毫秒为单位的 Unix 时间戳）
			/// </summary>
			public long betEndTime { get; set; }
			/// <summary>
			/// 表示旋转类型：
			///	True：特色旋转
			///	False：普通旋转
			/// </summary>
			public bool isFeatureBuy { get; set; }
		}


		public class Error
		{
			public string code { get; set; }
			public string message { get; set; }
			public string traceId { get; set; }
		}
	}
}