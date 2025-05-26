namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Request
{
    public class GetBalanceRequest : AERequestBase
    {
        public override string action => "get_balance";

        public string account_name { get; set; }
    }
}
