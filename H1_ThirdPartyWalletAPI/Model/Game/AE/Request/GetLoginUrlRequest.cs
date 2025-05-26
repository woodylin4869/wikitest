namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Request
{
    public class GetLoginUrlRequest : AERequestBase
    {
        public override string action => "register_token";
        public string account_name { get; set; }
        public int game_id { get; set; }
        public string lang { get; set; }
        public string exit_url { get; set; }
        public int noFullscreen { get; set; }

    }
}
