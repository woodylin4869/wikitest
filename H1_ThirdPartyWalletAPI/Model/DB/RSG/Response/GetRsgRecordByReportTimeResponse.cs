namespace H1_ThirdPartyWalletAPI.Model.DB.RSG.Response
{
    public class GetRsgRecordByReportTimeResponse : RSGRecordPrimaryKey
    {
        public decimal betamt { get; set; }
        public decimal winamt { get; set; }
        public decimal jackpotwin { get; set; }
    }
}
