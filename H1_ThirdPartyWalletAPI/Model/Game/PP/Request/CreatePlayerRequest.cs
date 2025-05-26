namespace H1_ThirdPartyWalletAPI.Model.Game.PP.Request
{
    public class CreatePlayerRequest
    {
        public string secureLogin { get; set; }
        /// <summary>
        /// 會員ID
        /// </summary>
        public string externalPlayerId { get; set; }
        /// <summary>
        /// 玩家貨幣
        /// </summary>
        public string currency { get; set; }
    }
}
