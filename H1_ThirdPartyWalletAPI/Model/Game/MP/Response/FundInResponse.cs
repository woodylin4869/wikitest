namespace H1_ThirdPartyWalletAPI.Model.Game.MP.Response
{
    public class FundInResponse
    {
        public string m { get; set; }
        public int s { get; set; }
        public FundIn d { get; set; }


        public class FundIn
        {
            public int code { get; set; }
            public string money { get; set; }
        }
    }
}
