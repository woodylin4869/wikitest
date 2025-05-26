namespace H1_ThirdPartyWalletAPI.Model.Game.WM.Request
{
    public class GetMemberTradeReportRequest
    {
        public string cmd { get; set; }
        public string vendorId { get; set; }
        public string signature { get; set; }
        public string user { get; set; }
        public string order { get; set; }
        public long timestamp { get; set; }
    }
}
