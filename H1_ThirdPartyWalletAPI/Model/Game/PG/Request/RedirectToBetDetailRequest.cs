namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 投注详情页面
    /// </summary>
    public class RedirectToBetDetailRequest
    {
        /// <summary>
        /// 请求的唯一标识符（GUID)
        /// 注: • 请把参数值设置为 GUID 格式
        /// </summary>
        public string trace_id { get; set; }
        /// <summary>
        /// 运营商令牌
        /// 呼叫 LoginProxy API 取得
        /// </summary>
        public string t { get; set; }
        /// <summary>
        /// 母注单 ID
        /// </summary>
        public string psid { get; set; }
        /// <summary>
        /// 子投注 ID
        /// </summary>
        public string sid { get; set; }
        /// <summary>
        /// 游戏 ID
        /// </summary>
        public string gid { get; set; }
        /// <summary>
        /// 语言：
        ///	• en：英文 （默认）
        ///	• zh：中文
        /// </summary>
        public string lang { get; set; }
        /// <summary>
        /// 固定值：operator
        /// </summary>
        public string type { get; set; }
	}
}