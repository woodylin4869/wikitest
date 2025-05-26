namespace H1_ThirdPartyWalletAPI.Model.Game.WM.Response
{
    public class WMBalanceResponse
    {

        public int errorCode { get; set; }
        public string errorMessage { get; set; }
        public Result result { get; set; }

        public class Result
        {
            public string yourOrderNum { get; set; }
            public string orderId { get; set; }
            public string cash { get; set; }
        }

    }
}
