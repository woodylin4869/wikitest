namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    public class GetTransactionHistoryResultRequest : SexyRequestBase
    {
        public string userId { get; set; }
        public string platform { get; set; }
        public string platformTxId { get; set; }
        public string roundId { get; set; }

    }
}
