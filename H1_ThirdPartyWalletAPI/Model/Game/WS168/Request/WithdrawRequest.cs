namespace H1_ThirdPartyWalletAPI.Model.Game.WS168.Request
{
    public class WithdrawRequest
    {
        public string account { get; set; }
        public string merchant_order_num { get; set; }
        public decimal amount { get; set; }
    }
}
