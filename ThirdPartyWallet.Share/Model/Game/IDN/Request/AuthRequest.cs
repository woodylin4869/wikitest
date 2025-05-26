using ThirdPartyWallet.Share.Model.Game.IDN.Response;

namespace ThirdPartyWallet.Share.Model.Game.IDN.Request
{
    public class AuthRequest
    {
        /// <summary>
        /// 	Grant Access type use `client_credentials` for client grant
        /// </summary>
        public string grant_type { get; set; }

        /// <summary>
        /// Client Id
        /// </summary>
        public string client_id { get; set; }

        /// <summary>
        /// Client Secret
        /// </summary>
        public string client_secret { get; set; }
        public string scope { get; set; }
    }

}
