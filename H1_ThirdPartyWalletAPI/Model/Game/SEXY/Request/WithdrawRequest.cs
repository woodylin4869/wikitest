namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    public class WithdrawRequest : SexyRequestBase
    {
        public string userId { get; set; }
        public string txCode { get; set; }
        public string withdrawType { get; set; }
        public decimal transferAmount { get; set; }
    }
}
