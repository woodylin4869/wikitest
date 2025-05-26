namespace H1_ThirdPartyWalletAPI.Model.DB.RLG.Response
{
    public class GetRlgRunningRecordResponse : GetRlgRecordBySummaryResponse
    {
        public string club_id { get; set; }
        public string franchiser_id { get; set; }
    }
}
