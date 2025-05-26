namespace ThirdPartyWallet.Share.Model.Game.IDN.Request
{
    public class LaunchRequest
    {
        /// <summary>
        /// Optional parameter to launch create game user
        /// </summary>
        public string create { get; set; }

        /// <summary>
        ///launch_url
        /// </summary>
        public string lobbyUrl { get; set; }

        public string lang { get; set; }
    }
}