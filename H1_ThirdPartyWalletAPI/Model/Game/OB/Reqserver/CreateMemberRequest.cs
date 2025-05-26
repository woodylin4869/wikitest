namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Reqserver
{
    public class CreateMemberRequest
    {
        public string loginName { set; get; }

        public string loginPassword { set; get; }

        public int lang { set; get; }
        public int oddType { set; get; }

        public string timestamp { set; get; }
    }
}
