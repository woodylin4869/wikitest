using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 获取多个玩家钱包余额
    /// </summary>
    public class GetPlayersWalletRequest
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
        public List<string> player_names { get; set; }
    }
}