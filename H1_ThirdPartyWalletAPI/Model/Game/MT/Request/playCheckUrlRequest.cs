namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Request
{
    public class playCheckUrlRequest
    {
        public string merchantId { get; set; }
        public string code { get; set; }
        public string data { get; set; }
    }

    public class playCheckUrlrawData
    {
        public string rowID { get; set; }
        public string lang { get; set; }

    }
}
