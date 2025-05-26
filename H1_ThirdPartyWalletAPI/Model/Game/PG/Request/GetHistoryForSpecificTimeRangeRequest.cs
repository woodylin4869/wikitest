namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 获取特定时间内的历史记录
    /// </summary>
    public class GetHistoryForSpecificTimeRangeRequest
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
        /// 每批的记录数
        /// 注： 值度 1500 - 5000
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 投注类型的投注记录：
        /// 1: 真实游戏
        /// </summary>
        public int bet_type { get; set; }
        /// <summary>
        /// 投注记录开始时间（以毫秒为单位的 Unix 时间戳）
        /// 注： 值度为 1 - 40 天
        /// </summary>
        public long from_time { get; set; }
        /// <summary>
        /// 投注记录结束的时间（以毫秒为单位的 Unix 时间戳）
        /// 注： 值度为 1 - 40 天
        /// </summary>
        public long to_time { get; set; }
	}
}