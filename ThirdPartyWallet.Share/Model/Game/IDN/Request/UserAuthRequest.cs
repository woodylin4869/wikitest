namespace ThirdPartyWallet.Share.Model.Game.IDN.Request
{
    public class UserAuthRequest
    {
        /// <summary>
        /// 	Username of player, please use format {whitelabelCode}{username}
        /// </summary>
        public string username { get; set; }

        /// <summary>
        /// 	Password of players
        /// </summary>
        public string password { get; set; }
    }

}
