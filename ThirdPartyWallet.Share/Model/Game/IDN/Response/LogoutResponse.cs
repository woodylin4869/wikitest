namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class LogoutResponse
    {
        public bool success { get; set; }
        public int response_code { get; set; }
        public string message { get; set; }
        public object[] data { get; set; }
        public object errors { get; set; }
    }


}