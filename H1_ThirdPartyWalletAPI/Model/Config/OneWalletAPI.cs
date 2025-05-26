namespace H1_ThirdPartyWalletAPI.Model.Config
{
    public class DBConnection
    {
        public WalletConnection Wallet { get; set; }
        public BetLogConnection BetLog { get; set; }

        public ClickHouseDBConnection ClickHouse { get; set; }
    }

    public class WalletConnection
    {
        public string Master { get; set; }
        public string Read { get; set; }
    }

    public class BetLogConnection
    {
        public string Master { get; set; }
        public string Read { get; set; }
    }
    public class ClickHouseDBConnection
    {
        public string Master { get; set; }
    }
    

    public class OneWalletAPI
    {
        public DBConnection DBConnection { get; set; }
        public string WalletMode { get; set; }
        public string RCGMode { get; set; }
        public string OpenGame { get; set; }
        public string Prefix_Key { get; set; }
        public string Redis_PreKey { get; set; }
        public string Slack_Channel { get; set; }
        
    }
}