namespace H1_ThirdPartyWalletAPI.Model.DB.NEXTSPIN.Response
{
    public class GetNextSpinRecordV2sBySummaryResponse : NextSpinPrimaryKey
    {
        public decimal betAmount { get; set; }

        public decimal winLoss { get; set; }

        public string gameCode { get; set; }
    }
}
