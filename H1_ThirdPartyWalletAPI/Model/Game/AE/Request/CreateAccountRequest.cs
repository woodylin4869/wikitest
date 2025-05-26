namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Request
{
    public class CreateAccountRequest : AERequestBase
    {
        public override string action => "create_account";
        public string account_name { get; set; }
        public string currency { get; set; }
    }
}
