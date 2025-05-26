namespace H1_ThirdPartyWalletAPI.Model.Game.WM.Request
{
    public class SigninGameRequest
    {
        public string cmd { get; set; }
        public string vendorId { get; set; }
        public string signature { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public int lang { get; set; }
        public string returnurl { get; set; }
        public long timestamp { get; set; }
    }
}
