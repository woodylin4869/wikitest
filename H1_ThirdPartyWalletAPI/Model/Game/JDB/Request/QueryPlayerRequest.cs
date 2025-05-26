namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class QueryPlayerRequest : RequestBaseModel
    {
        public override int Action => 15;
        public string Uid { get; set; }
    }
}
