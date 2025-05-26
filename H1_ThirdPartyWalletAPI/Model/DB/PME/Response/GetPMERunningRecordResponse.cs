namespace H1_ThirdPartyWalletAPI.Model.DB.PME.Response
{
    public class GetPMERunningRecordResponse : GetPMERecordsBySummaryResponse
    {
        public string club_id { get; set; }

        public string franchiser_id { get; set; }
    }
}
