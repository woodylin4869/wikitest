namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 获取单一玩家的历史记录
    /// </summary>
    public class GetPlayerHistoryRequest
    {
        /// <summary>
        /// 运营商的唯一标识符
        /// </summary>
		public string operator_token { get; set; }
        /// <summary>
        /// PGSoft 和运营商之间的共享密码
        /// </summary>
        public string secret_key { get; set; }
        /// <summary>
        /// 玩家的唯一标识符
        /// </summary>
        public string player_name { get; set; }
        /// <summary>
        /// 投注类型的投注记录：
        /// 1: 真实游戏
        /// </summary>
        public int bet_type { get; set; }
        /// <summary>
        /// 投注记录开始的时间（以毫秒为单位的 Unix 时间戳）
        /// 注： 值度 1 - 7 天
        /// </summary>
        public long start_time { get; set; }
        /// <summary>
        /// 投注记录结束的时间（以毫秒为单位的 Unix 时间戳）
        /// 注： 值度 1 - 7 天
        /// </summary>
        public long end_time { get; set; }
	}
}