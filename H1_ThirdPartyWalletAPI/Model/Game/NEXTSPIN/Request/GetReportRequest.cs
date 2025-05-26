namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Request
{
    public class GetReportRequest : BaseRequest
    {
            public string beginDate { get; set; }
            public string endDate { get; set; }
            public string merchantCode { get; set; }
            public int pageIndex { get; set; }
    }
}
