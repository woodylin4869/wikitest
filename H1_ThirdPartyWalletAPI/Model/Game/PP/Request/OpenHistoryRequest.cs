namespace H1_ThirdPartyWalletAPI.Model.Game.PP.Request
{
    public class OpenHistoryRequest
    {
        public string secureLogin { get; set; }
        public string playerId { get; set; }
        public long roundId { get; set; }
    }
}
