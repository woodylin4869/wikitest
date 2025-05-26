namespace ThirdPartyWallet.Share.Model.Game.CR.Request
{
    public class AGLoginRequest
    {
        public string username { get; set; }
        public string password { get; set; }
        public string remoteip { get; set; }
        public string method { get; set; }
        public long timestamp { get; set; }
    }

}