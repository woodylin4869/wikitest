namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 查询玩家的钱包余额
    /// </summary>
    public class GetPlayerWalletRequest
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
        /// 玩家独有的身份识别
        /// </summary>
        public string player_name { get; set; }
    }
}