namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Request
{
    public class CreateMemberRequest
    {
        public string LoginName { get; set; }
        public string LoginPassword { get; set; }
        public int oddType { get; set; }
        public string Lang { get; set; }
    }
}
