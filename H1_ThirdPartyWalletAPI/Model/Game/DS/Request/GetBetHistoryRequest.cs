namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Request
{
    public class GetBetHistoryRequest : RequestBaseModel
    {
        public string account { get; set; }

        public string Game_id { get; set; }

        public string Lang { get; set; }
    }

}
