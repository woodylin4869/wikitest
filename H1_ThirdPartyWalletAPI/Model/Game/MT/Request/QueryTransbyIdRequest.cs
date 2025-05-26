namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Request
{
    public class QueryTransbyIdRequest
    {
        public string playerName { get; set; }
        public string merchantId { get; set; }
        public string extTransId { get; set; }
        public string data { get; set; }
    }
    public class QueryTransbyIdrawData
    {
        public string currency { get; set; }
    }
}
