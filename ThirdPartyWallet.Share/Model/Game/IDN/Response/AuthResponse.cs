using ThirdPartyWallet.Share.Model.Game.IDN.Response;

namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class AuthResponse : AuthResponseBase
    {
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string access_token { get; set; }
    }
}