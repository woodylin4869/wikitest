namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Response
{
    public class WithdrawResponse
    {

        public string code { get; set; }
        public string message { get; set; }
        public Request request { get; set; }
        public Data data { get; set; }


        public class Request
        {
            public string loginName { get; set; }
            public string transferNo { get; set; }
            public decimal amount { get; set; }
            public long timestamp { get; set; }
        }

        public class Data
        {
            public string deposit { get; set; }
        }
    }
}
