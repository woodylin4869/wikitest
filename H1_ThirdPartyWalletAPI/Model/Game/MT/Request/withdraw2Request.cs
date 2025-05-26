namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Request
{
    public class withdraw2Request
    {
        public string merchantId { get; set; }
        public string playerName { get; set; }
        public string coins { get; set; }
        public string extTransId { get; set; }
        public string code { get; set; }
        public string data { get; set; }
    }

    public class withdraw2rawData
    {
        public string merchantId { get; set; }

        public string playerName { get; set; }

        public string extTransId { get; set; }

        public string coins { get; set; }
        public string currency { get; set; }
    }
}
