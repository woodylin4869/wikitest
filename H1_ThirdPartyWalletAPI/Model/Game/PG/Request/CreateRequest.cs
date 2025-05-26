namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 创建玩家账号
    /// </summary>
    public class CreateRequest
    {
        /// <summary>
        /// 运营商独有的身份识别
        /// </summary>
        public string operator_token { get; set; }
        /// <summary>
        /// PGSoft 与运营商之间共享密码
        /// </summary>
        public string secret_key { get; set; }
        /// <summary>
        /// 玩家帐号，注: 上限 50 字符
        /// </summary>
        public string player_name { get; set; }
        /// <summary>
        /// 玩家昵称，注: 上限 50 字符
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 玩家选择的币种
        /// </summary>
        public string currency { get; set; }
	}
}