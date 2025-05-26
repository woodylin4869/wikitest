namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Request
{
    public class PlayerPlatformUrlRequest
    {
        public string merchantId { get; set; }
        public string playerName { get; set; }
        public string pwd { get; set; }
        public string code { get; set; }
        public string data { get; set; }

    }

    public class PlayerPlatformUrlrawData
    {
        public string gameHall { get; set; }
        public string gameCode { get; set; }
        public string lang { get; set; }
    }
}
