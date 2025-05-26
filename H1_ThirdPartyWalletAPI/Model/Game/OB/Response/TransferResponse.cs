namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Response
{
    public class TransferResponse
    {

            public string code { get; set; }
            public string message { get; set; }
            public Request request { get; set; }
            public Data data { get; set; }
        

        public class Request
        {
            public string transferNo { get; set; }
            public string loginName { get; set; }
            public long timestamp { get; set; }
        }

        public class Data
        {
            public string tradeNo { get; set; }
            public decimal amount { get; set; }
            public int transferStatus { get; set; }
        }

    }
}
