namespace H1_ThirdPartyWalletAPI.Model.Game.PP.Request
{
    public class StartGameRequest
    {
        public string secureLogin { get; set; }
        public string externalPlayerId { get; set; }
        public string platform { get; set; }
        public string gameId { get; set; }
        public string language { get; set; }
        public string lobbyURL { get; set; }
    }
}
