namespace H1_ThirdPartyWalletAPI.Model.Game.MP.Response
{
    public class LoginToPlatformResponse
    {

        public string m { get; set; }
        public int s { get; set; }
        public LoginToPlatform d { get; set; }
        public class LoginToPlatform
        {
            public int code { get; set; }
            public string url { get; set; }
        }

    }
}
