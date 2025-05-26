namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Request
{
    public class GetGameHistoryUrlRequest : AERequestBase
    {
        public override string action => "get_game_history_url";
        public string account_name { get; set; }
        public string round_id { get; set; }
        public string lang { get; set; }
    }
}
