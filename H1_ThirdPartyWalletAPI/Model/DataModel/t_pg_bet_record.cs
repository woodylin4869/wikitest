using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class t_pg_bet_record
    {
		public Guid summary_id { get; set; }
		/// <summary>
		/// 母注单的唯一标识符
		/// </summary>
		public long parentbetid { get; set; }
		/// <summary>
		/// 子投注的唯一标识符 （唯一键值）
		/// </summary>
		public long betid { get; set; }
		/// <summary>
		/// 玩家的唯一标识符
		/// </summary>
		public string playername { get; set; }
		/// <summary>
		/// 游戏的唯一标识符
		/// </summary>
		public int gameid { get; set; }
		/// <summary>
		/// 投注记录类别：
		/// 1: 真实游戏
		/// </summary>
		public int bettype { get; set; }
		/// <summary>
		/// 交易类别：
		///	1: 现金
		///	2: 红利
		///	3: 免费游戏
		/// </summary>
		public int transactiontype { get; set; }
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
		public decimal betamount { get; set; }
		/// <summary>
		/// 玩家的所赢金额
		/// </summary>
		public decimal winamount { get; set; }
		/// <summary>
		/// 玩家的奖池返还率贡献额
		/// </summary>
		public decimal jackpotrtpcontributionamount { get; set; }
		/// <summary>
		/// 玩家的奖池贡献额
		/// </summary>
		public decimal jackpotcontributionamount { get; set; }
		/// <summary>
		/// 玩家的奖池金额
		/// </summary>
		public decimal jackpotwinamount { get; set; }
		/// <summary>
		/// 玩家交易前的余额
		/// </summary>
		public decimal balancebefore { get; set; }
		/// <summary>
		/// 玩家交易后的余额
		/// </summary>
		public decimal balanceafter { get; set; }
		/// <summary>
		/// 投注状态：
		///	1: 非最后一手投注
		///	2：最后一手投注
		///	3：已调整
		/// hand_status=1 這裡是指這輪遊戲還未結束，還會有下一輪遊戲
		/// 例子:
		/// 比方說，玩家在遊戲贏得1個免費旋轉。
		/// 1000001 是玩家的投注記錄，
		/// 1000002 是玩家的免費旋轉。
		/// 1000001 會有 hand_status 1，
		/// 1000002 會有hand_status 2。
		/// 1000003 是玩家接續下注，但沒有贏取任何金額，
		/// 1000003 的hand status會是 2
		/// 若玩家超過90天（默認）沒有登陸游戲，所有遊戲將會被重置。這時，您會有一條記錄是 hand status=3
		/// </summary>
		public int handsstatus { get; set; }
		/// <summary>
		/// 数据更新时间（以毫秒为单位的 Unix 时间戳）
		/// </summary>
		public DateTime rowversion { get; set; }
		/// <summary>
		/// 当前投注的开始时间（以毫秒为单位的 Unix 时间戳）
		/// </summary>
		public DateTime bettime { get; set; }
		/// <summary>
		/// 当前投注的结束时间（以毫秒为单位的 Unix 时间戳）
		/// </summary>
		public DateTime betendtime { get; set; }
		/// <summary>
		/// 表示旋转类型：
		///	True：特色旋转
		///	False：普通旋转
		/// </summary>
		public bool isfeaturebuy { get; set; }
	}
}