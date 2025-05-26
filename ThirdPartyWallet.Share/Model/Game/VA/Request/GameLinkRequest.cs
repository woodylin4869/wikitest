namespace ThirdPartyWallet.Share.Model.Game.VA.Request
{
    public class GameLinkRequest
    {
        // Channel ID - Required, number
        public string ChannelId { get; set; }

        // Account - Required, string (3 to 32 characters)
        public string Account { get; set; }

        // Currency - Required, string
        public string Currency { get; set; }

        // Game ID - Required, number (0: Main Lobby, Non-zero: Specific Game ID)
        public string GameId { get; set; }

        // Game Platform - Required, string (web or mobile)
        public string GamePlat { get; set; }

        // App - Required, string (Y or N)
        public string App { get; set; }

        // Language - Required, string
        public string Lang { get; set; }

        // URL - Optional, string (3 to 100 characters, must start with http:// or https://)
        public string Url { get; set; }

    }
}
