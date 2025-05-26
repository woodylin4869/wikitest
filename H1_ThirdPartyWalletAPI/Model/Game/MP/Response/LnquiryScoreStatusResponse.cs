namespace H1_ThirdPartyWalletAPI.Model.Game.MP.Response
{
    public class LnquiryScoreStatusResponse
    {

        public string m { get; set; }
        public int s { get; set; }
        public LnquiryScoreStatus d { get; set; }


        public class LnquiryScoreStatus
        {
            public int code { get; set; }
            public string account { get; set; }
            public string totalMoney { get; set; }
            public string freeMoney { get; set; }
            public int status { get; set; }
            public int gameStatus { get; set; }
        }
    }
}
