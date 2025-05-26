namespace ThirdPartyWallet.Share.Model.Game.VA
{
    public class VAConfig
    {
        public const string ConfigKey = "VAConfig";
        public string? VA_Authorization { get; set; }
        public string? VA_KEY { get; set; }

        public string? VA_channelId { get; set; }

        public string? VA_URL { get; set; }
    }
}
