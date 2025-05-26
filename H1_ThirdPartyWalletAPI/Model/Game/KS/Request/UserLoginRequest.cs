namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Request
{
    public class UserLoginRequest
    {
        /// <summary>
        /// 登录的会员名，示例：ceshi01
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 可选，要进入的游戏。如果留空或者游戏ID错误，则默认进入游戏大厅首页，游戏ID代码请参见附录1
        /// </summary>
        public int? CateID { get; set; }

        /// <summary>
        /// 可选，要进入的赛事。如果留空或者比赛ID错误，则默认进入游戏大厅首页。
        /// </summary>
        public int? MatchID { get; set; }
    }
}
