namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 令牌验证
    /// </summary>
    public class VerifySessionRequest
    {
		/// <summary>
		/// 请求的验证
		/// </summary>
        public string traceId { get; set; }
        /// <summary>
        /// 运营商独有的身份识别
        /// </summary>
        public string operator_token { get; set; }
        /// <summary>
        /// PGSoft 与运营商之间共享密码
        /// </summary>
        public string secret_key { get; set; }
        /// <summary>
        /// 运营商系统生成的令牌
        ///	注：
        ///	• 上限 200 字符
        ///	• 请使用 UrlDecode 解码参数值，以避免发生未知错误。
        /// </summary>
        public string operator_player_session { get; set; }
        /// <summary>
        /// 玩家 IP 地址
        /// </summary>
        public string ip { get; set; }
        /// <summary>
        /// URL scheme18中的operator_param 值
        /// </summary>
        public string custom_parameter { get; set; }
        /// <summary>
        /// 游戏的独有代码
        /// </summary>
        public string game_id { get; set; }
	}
}