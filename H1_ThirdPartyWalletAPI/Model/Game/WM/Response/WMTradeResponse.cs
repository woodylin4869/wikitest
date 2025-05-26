namespace H1_ThirdPartyWalletAPI.Model.Game.WM.Response
{
    public class WMTradeResponse
    {

        public int errorCode { get; set; }
        public string errorMessage { get; set; }
        public Result[] result { get; set; }


        public class Result
        {
            public string orderid { get; set; }
            public string addtime { get; set; }
            public string money { get; set; }
            public string op_code { get; set; }
            public string subtotal { get; set; }
            public string ordernum { get; set; }
            public string user { get; set; }
        }

    }
}
