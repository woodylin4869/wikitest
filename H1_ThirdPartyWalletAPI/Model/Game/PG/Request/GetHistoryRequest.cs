namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    public class GetHistoryRequest
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
        ///	1: 真实游戏 
        /// </summary>
        public int bet_type { get; set; }
        /// <summary>
        /// 数据更新时间（以毫秒为单位的 Unix 时间戳）
        ///	注：
        ///	• 首次呼叫时将值设置为 1
        ///	• 运营商需保存每个呼叫最大的row_version 以用作下一个呼叫请求的 row_version 值
        /// </summary>
        public long row_version { get; set; }
        /// <summary>
        /// 投注状态：
        ///	0:全部（默认）
        ///	1: 非最后一手投注
        ///	2：最后一手投注
        ///	3：已调整
        /// </summary>
        public long hands_status { get; set; }
	}
}