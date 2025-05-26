namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class KickOutRequest: RequestBaseModel
    {
        public override int Action => 17;
        public string Uid { get; set; }
    }
}
