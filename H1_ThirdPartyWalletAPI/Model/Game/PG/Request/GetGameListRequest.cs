namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 获取游戏列表
    /// </summary>
    public class GetGameListRequest
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
        /// 游戏合法投注金额的货币
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// 数据内容的语言
        /// en-us: 英文
        /// zh-cn: 中文
        /// </summary>
        public string language { get; set; }
        /// <summary>
        /// 游戏状态：
        ///	0：无效的游戏
        ///	1：活跃的游戏
        ///	空值：无效和活跃的游戏都将被选择
        /// </summary>
        public int status { get; set; }
	}
}