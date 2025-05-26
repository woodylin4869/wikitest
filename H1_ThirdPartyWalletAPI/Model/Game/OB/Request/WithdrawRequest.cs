namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Request
{
    public class WithdrawRequest
    {
        public string loginName { get; set; }
        public string transferNo { get; set; }
        public decimal amount { get; set; }
    }
}
