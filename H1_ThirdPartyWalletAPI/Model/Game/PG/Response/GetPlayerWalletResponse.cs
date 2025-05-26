using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
    /// <summary>
    /// 查询玩家的钱包余额
    /// </summary>
    public class GetPlayerWalletResponse
    {
		public Data data { get; set; }
		public Error error { get; set; }

		public class Data
		{
			/// <summary>
			/// 玩家选择的币种
			/// </summary>
			public string currencyCode { get; set; }
			/// <summary>
			/// 所有玩家钱包余额的总和：
			///	现金余额
			///	红利余额
			///	免费游戏余额
			/// </summary>
			public decimal totalBalance { get; set; }
			/// <summary>
			/// 玩家的现金钱包余额
			/// 运营商可参考该值作为玩家余额
			/// </summary>
			public decimal cashBalance { get; set; }
			/// <summary>
			/// 玩家的红利余额
			/// 这将显示玩家的所有可用红利余额，它无法被转出。
			/// </summary>
			public decimal totalBonusBalance { get; set; }
			/// <summary>
			/// 玩家的免费游戏钱包余额
			/// 这将显示玩家的所有可用的免费游戏，它无法被转出。
			/// </summary>
			public decimal freeGameBalance { get; set; }
			/// <summary>
			/// 玩家的红利详情
			/// </summary>
			public List<bonuses> bonuses { get; set; }
			/// <summary>
			/// 玩家的免费游戏详情
			/// </summary>
			public List<freeGames> freeGames { get; set; }
			/// <summary>
			/// 玩家的现金钱包详情
			/// </summary>
			public cashWallet cashWallet { get; set; }
			/// <summary>
			/// 玩家的红利钱包详情
			/// </summary>
			public bonusWallet bonusWallet { get; set; }
			/// <summary>
			/// 玩家的免费游戏钱包详情
			/// </summary>
			public freeGameWallet freeGameWallet { get; set; }
		}

		/// <summary>
		/// 玩家的现金钱包详情
		/// </summary>
		public class cashWallet
		{
			/// <summary>
			/// 现金钱包的唯一标识符
			/// </summary>
			public string key { get; set; }
			/// <summary>
			/// 玩家现金钱包的唯一标识符
			/// </summary>
			public int cashId { get; set; }
			/// <summary>
			/// 玩家的现金余额
			/// </summary>
			public decimal cashBalance { get; set; }
		}

		public class bonuses
		{
			/// <summary>
			/// 红利钱包的唯一标识符
			/// </summary>
			public string key { get; set; }
			/// <summary>
			/// 红利钱包的唯一标识符
			/// 只有在红利游戏分配给玩家后才会出现
			/// </summary>
			public int bonusId { get; set; }
			/// <summary>
			/// 红利游戏名称
			/// 只有在红利游戏分配给玩家后才会出现
			/// </summary>
			public string bonusName { get; set; }
			/// <summary>交易的唯一标识符</summary>
			public string transactionId { get; set; }
			/// <summary>
			/// 母红利类型：
			/// 红利：正常创建的红利游戏
			/// 免费游戏：从免费游戏转换的红利游戏
			/// </summary>
			public string bonusParentType { get; set; }
			/// <summary>
			/// 游戏的独有代码
			/// </summary>
			public List<int> gameIds { get; set; }
			/// <summary>
			/// 未完成的游戏 ID
			/// </summary>
			public int gameIdLock { get; set; }
			/// <summary>
			/// 玩家的红利余额
			/// </summary>
			public decimal balanceAmount { get; set; }
			/// <summary>
			/// 红利游戏的流水要求
			/// 指红利游戏里的所赢金额在进入红利钱包之后，需要完成的总金额
			/// </summary>
			public decimal bonusRatioAmount { get; set; }
			/// <summary>
			/// 红利游戏的最低现金转换金额
			/// </summary>
			public decimal minimumConversionAmount { get; set; }
			/// <summary>
			/// 红利游戏的最高现金转换金额
			/// </summary>
			public decimal maximumConversionAmount { get; set; }
			/// <summary>
			/// 红利钱包的状态：
			///	0：无效（已取消）
			///	1：活跃
			///	2：已过期
			///	3：已转换
			///	4：已完成
			///	5：全新
			/// 6：已取消用户
			/// 7：已锁定
			/// 8：待处理
			/// </summary>
			public int status { get; set; }
			/// <summary>
			/// 红利游戏的创建日期(Unix 时间戳，以毫秒为单位)
			/// </summary>
			public long createdDate { get; set; }
			/// <summary>
			/// 红利游戏的截止日期(Unix 时间戳，以毫秒为单位)
			/// </summary>
			public long expiredDate { get; set; }
			/// <summary>
			/// 创建红利游戏的 API 或 BackOffice 用户
			/// </summary>
			public string createdBy { get; set; }
			/// <summary>
			/// 更新红利游戏的 API 或 BackOffice 用户
			/// </summary>
			public string updatedBy { get; set; }
			/// <summary>
			/// 允许玩家取消优惠：
			/// True: 不允许玩家取消优惠
			/// False: 允许玩家取消优惠
			/// </summary>
			public bool isSuppressDiscard { get; set; }
		}

		public class freeGames
		{
			/// <summary>
			/// 免费游戏钱包的唯一标识符
			/// </summary>
			public string key { get; set; }
			/// <summary>
			/// 免费游戏的唯一标识符
			/// 只有在免费游戏游戏分配给玩家后才会出现
			/// </summary>
			public int freeGameId { get; set; }
			/// <summary>
			/// 免费游戏名称
			/// 只有在免费游戏游戏分配给玩家后才会出现
			/// </summary>
			public string freeGameName { get; set; }
			/// <summary>
			/// 交易的唯一标识符
			/// </summary>
			public string transactionId { get; set; }
			/// <summary>
			/// 游戏的独有代码
			/// </summary>
			public List<int> gameIds { get; set; }
			/// <summary>
			/// 未完成的游戏 ID
			/// </summary>
			public int gameIdLock { get; set; }
			/// <summary>
			/// 剩余的免费游戏总数
			/// </summary>
			public int gameCount { get; set; }
			/// <summary>
			/// 免费游戏总数
			/// </summary>
			public int totalGame { get; set; }
			/// <summary>
			/// 玩家的免费游戏余额
			/// </summary>
			public decimal balanceAmount { get; set; }
			/// <summary>
			/// 免费游戏的最低现金转换金额
			/// </summary>
			public decimal minimumConversionAmount { get; set; }
			/// <summary>
			/// 免费游戏的最高现金转换金额
			/// </summary>
			public decimal maximumConversionAmount { get; set; }
			/// <summary>
			/// 免费游戏的投注倍数
			/// </summary>
			public int multiplier { get; set; }
			/// <summary>
			/// 免费游戏的筹码大小
			/// </summary>
			public decimal coinSize { get; set; }
			/// <summary>
			/// 免费游戏的创建时间(Unix 时间戳，以毫秒为单位)
			/// </summary>
			public long createdDate { get; set; }
			/// <summary>
			/// 免费游戏的截止时间(Unix 时间戳，以毫秒为单位)
			/// </summary>
			public long expiredDate { get; set; }
			/// <summary>
			/// 免费游戏钱包的状态：
			///	0：无效（已取消）
			///	1：活跃
			///	2：已过期
			///	3：已转换
			///	4：已完成
			///	5：全新
			///	6：已取消用户
			///	7：已封锁
			///	8：待处理
			/// </summary>
			public int status { get; set; }
			/// <summary>
			/// 免费游戏完成后的转换类型：
			/// C：现金
			/// B：红利
			/// </summary>
			public string conversionType { get; set; }
			/// <summary>
			/// 在 API 或 BackOffice 创建免费游戏的用户
			/// </summary>
			public string createdBy { get; set; }
			/// <summary>
			/// 在 API 或 BackOffice 更新免费游戏的用户
			/// </summary>
			public string updatedBy { get; set; }
			/// <summary>
			/// 允许玩家取消优惠：
			/// True: 不允许玩家取消优惠
			/// False: 允许玩家取消优惠
			/// </summary>
			public bool isSuppressDiscard { get; set; }
		}

		public class bonusWallet
		{
			public decimal totalBonusBalance { get; set; }
			public List<bonuses> bonuses { get; set; }
		}

		public class freeGameWallet
		{
			public decimal freeGameBalance { get; set; }
			public List<freeGames> freeGames { get; set; }
		}

		public class Error
		{
			/// <summary>
			/// 3001 不能空值
			/// 3005 玩家钱包不存在
			/// </summary>
			public string code { get; set; }
			public string message { get; set; }
			public string traceId { get; set; }
		}
	}
}