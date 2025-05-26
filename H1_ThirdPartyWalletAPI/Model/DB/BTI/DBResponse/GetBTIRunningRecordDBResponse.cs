namespace H1_ThirdPartyWalletAPI.Model.DB.BTI.DBResponse
{
    public class GetBTIRunningRecordDBResponse : GetBTIRecordsBySummaryDBResponse
    {
        public string club_id { get; set; }

        public string franchiser_id { get; set; }
    }
}
