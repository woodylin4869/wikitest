using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    public class AuthorizeRequest
    {
        public string token { get; set; }

        public string secret_key { get; set; }
    }
}
