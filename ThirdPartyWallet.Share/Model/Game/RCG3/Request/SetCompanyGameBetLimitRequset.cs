namespace ThirdPartyWallet.Share.Model.Game.RCG3.Request
{
    public class SetCompanyGameBetLimitRequset
    {
        public int gameID { get; set; }
        public string currency { get; set; }
        public Datalist[] dataList { get; set; }
    }

    public class Datalist
    {
        public int limitType { get; set; }
        public string betArea1 { get; set; }
        public string betArea2 { get; set; }
        public Layerbetlimit[] layerBetLimit { get; set; }
    }

    public class Layerbetlimit
    {
        public string h1SHIDString { get; set; }
        public int limit { get; set; }
    }
}
