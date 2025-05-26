using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
    /// <summary>
    /// 获取游戏列表
    /// </summary>
    public class GetGameListResponse
    {
		public List<Data> data { get; set; }
		public Error error { get; set; }

		public class Data
		{
			/// <summary>
			/// 游戏的唯一标识符
			/// </summary>
			public int gameId { get; set; }
			/// <summary>
			/// 游戏名称
			/// </summary>
			public string gameName { get; set; }
			/// <summary>
			/// 游戏的唯一识别码
			/// </summary>
			public string gameCode { get; set; }
			/// <summary>
			/// 游戏的合法投注金额
			/// </summary>
			public List<GameLegalBetAmount> gameLegalBetAmounts { get; set; }
			/// <summary>
			/// 全球游戏状态：
			///	0: 无效
			///	1: 活跃
			///	2: 已暂停
			///	注： 唯有在游戏状态和发布状态都显示活跃时才能进入游戏
			/// </summary>
			public int status { get; set; }
			/// <summary>
			/// 运营商的游戏状态：
			///	0: 无效
			///	1: 活跃
			///	2: 已暂停
			///	注： 唯有在游戏状态和发布状态都显示活跃时才能进入游戏
			/// </summary>
			public int releaseStatus { get; set; }
			/// <summary>
			/// 表示是否支持免费游戏：
			/// True：支持
			/// False：不支持
			/// </summary>
			public bool IsSupportFreeGame { get; set; }
			/// <summary>
			/// 游戏类型：
			/// 1：视频老虎机游戏
			/// 2：卡牌游戏
			/// </summary>
			public int category { get; set; }
		}

		public class GameLegalBetAmount
		{
			/// <summary>
			/// 游戏的唯一标识符
			/// </summary>
			public int gameId { get; set; }
			/// <summary>
			/// 游戏类型（仅限卡牌游戏）：
			///	0: 无
			///	1: 百家乐 – 免佣
			///	2: 百家乐 – 传统
			/// </summary>
			public int gameTypeId { get; set; }
			/// <summary>
			/// 游戏里可用的合法投注金额
			/// </summary>
			public List<legalBetAmount> legalBetAmounts { get; set; }
		}

		/// <summary>
		/// 游戏里可用的合法投注金额
		/// </summary>
		public class legalBetAmount
		{
			/// <summary>
			/// 游戏的投注大小
			/// </summary>
			public decimal coinSize { get; set; }
			/// <summary>
			/// 游戏的投注级别
			/// </summary>
			public int betMultiplier { get; set; }
			/// <summary>
			/// 游戏的基本投注
			/// </summary>
			public int baseBet { get; set; }
			/// <summary>
			/// 游戏的投注金额
			/// </summary>
			public decimal betAmount { get; set; }
		}


		public class Error
		{
			public string code { get; set; }
			public string message { get; set; }
			public string traceId { get; set; }
		}
	}
}