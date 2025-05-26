namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class CreatePlayerRequest : RequestBaseModel
    {
        public override int Action => 12;
        public string Uid { get; set; }

        public string Name { get; set; }

        public int Credit_allocated { get; set; }
    }
}
