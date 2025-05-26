namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    public class DepositRequest : SexyRequestBase
    {
        public string userId { get; set; }
        public decimal transferAmount { get; set; }
        public string txCode { get; set; }
    }
}
