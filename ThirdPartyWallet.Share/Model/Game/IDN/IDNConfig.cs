namespace ThirdPartyWallet.Share.Model.Game.IDN
{
    public class IDNConfig
    {
        public const string ConfigKey = "IDNConfig";

        public string IDN_ClientID { get; set; }
        public string IDN_ClientSecret { get; set; }
        public string IDN_whitelabel_code { get; set; }
        public string IDN_whitelabel_id { get; set; }
        public string IDN_payment_id { get; set; }

        public string IDN_currency_id { get; set; }

        public string IDN_URL { get; set; }
    }
}