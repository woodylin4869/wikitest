namespace H1_ThirdPartyWalletAPI.Model.Game.MP.Response
{
    public class FundOutResponse
    {
        public string m { get; set; }
        public int s { get; set; }
        public FundOut d { get; set; }


        public class FundOut
        {
            public int code { get; set; }
            public string money { get; set; }
        }
    }
}
