namespace H1_ThirdPartyWalletAPI.Model.Game.META.Request
{
    public class TransactionLogRequest
    {
        public string? Account { get; set; }

        public long Date { get; set; }
        public int? Limit { get; set; }
        public string? TranOrder { get; set; }

        public string? TradeOrder { get; set; }
    }
}
