namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class CheckDepositListResponse
    {
        public Deposits deposits { get; set; }
    }

    public class Deposits
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
        public object prev_page_url { get; set; }
        public int? to { get; set; }
        public int total { get; set; }
    }

    public class Datum
    {
        public int id { get; set; }
        public string bank { get; set; }
        public string accname { get; set; }
        public string accnumber { get; set; }
        public decimal amount { get; set; }

        /// <summary>
        /// 3=成功，只會顯示3
        /// </summary>
        public int status { get; set; }
        public int payment_id { get; set; }
        public int currency_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string param { get; set; }
        public object deposit_external_id { get; set; }
        public object deposit_external_id2 { get; set; }

        /// <summary>
        /// 我方的交易單號
        /// </summary>
        public string order_id { get; set; }
        public string type { get; set; }
    }

}