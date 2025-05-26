namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Request
{
    public class GetReportRequest : AERequestBase
    {
        public override string action => "get_winloss_summary";
        public string from_time { get; set; }
        public string to_time { get; set; }
        public int site_id { get; set; }
    }
}
