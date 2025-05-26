namespace H1_ThirdPartyWalletAPI.Model.DB.PP.Response
{
    public class GetPPRecordResponse: PPRecordPrimaryKey
    {
        public string ExtPlayerID { get; set; }
        public decimal pre_Bet { get; set; }
        public decimal pre_Win { get; set; }
    }
}
