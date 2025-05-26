namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class CheckWithdrawListResponse
    {
        public Withdraws withdraws { get; set; }
    }
    public class Withdraws
    {
        public int current_page { get; set; }
        public List<Datum> data { get; set; }
        public string first_page_url { get; set; }
        public int? from { get; set; }
        public int last_page { get; set; }
        public string last_page_url { get; set; }
        public object next_page_url { get; set; }
        public string path { get; set; }
        public int per_page { get; set; }
        //public object prev_page_url { get; set; }
        public int? to { get; set; }
        public int total { get; set; }
    }



}