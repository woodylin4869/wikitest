namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Request
{
    public class FastGameRequest
    {
        public string loginName { set; get; }
        public string loginPassword { set; get; }
        public int deviceType { set; get; }
        public int oddType { set; get; }
        public string Lang { set; get; }
        public string backurl { set; get; }
        public int showExit { set; get; }
        public int gameTypeId { set; get; }
    }
}
