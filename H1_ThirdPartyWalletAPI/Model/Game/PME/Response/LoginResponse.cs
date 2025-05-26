namespace H1_ThirdPartyWalletAPI.Model.Game.PME.Response
{
    public class LoginResponse : BaseResponse
    {
        public new Login data { get; set; }
    }

    public class Login
    {
        public string pc { get; set; }

        public string h5 { get; set; }

        public string token { get; set; }
    }
}
