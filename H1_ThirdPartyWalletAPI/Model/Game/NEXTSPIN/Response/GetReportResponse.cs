namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response
{
    public class GetReportResponse : BaseResponse
    {
        public List[] list { get; set; }
        public int pageCount { get; set; }
        public int resultCount { get; set; }
        public string merchantCode { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public string serialNo { get; set; }


        public class List
        {
            public string merchantCode { get; set; }
            public string currency { get; set; }
            public int betCount { get; set; }
            public decimal betAmount { get; set; }
            public decimal validBetAmount { get; set; }
            public decimal totalWL { get; set; }
            public decimal jpContributeAmt { get; set; }
            public decimal jpAmt { get; set; }
            public decimal jpTotalWL { get; set; }
            public decimal? bonusAmt { get; set; }
            public decimal? bonusBearAmt { get; set; }
        }
    }
}
