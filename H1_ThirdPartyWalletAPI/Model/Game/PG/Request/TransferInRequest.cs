namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 充值
    /// </summary>
    public class TransferInRequest
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
        /// 玩家帐号
        /// </summary>
        public string player_name { get; set; }
        /// <summary>
        /// 转账金额（小数点后两位）
        /// 注: 最低 0.01
        /// </summary>
        public decimal amount { get; set; }
        /// <summary>
        /// 交易凭证，运营商平台生成并确保每次交易是独有的。
        /// 注: 上限 50 字符
        /// </summary>
        public string transfer_reference { get; set; }
        /// <summary>
        /// 玩家选择的币种
        /// </summary>
        public string currency { get; set; }
	}
}