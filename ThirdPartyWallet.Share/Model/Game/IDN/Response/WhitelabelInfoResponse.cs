namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class WhitelabelInfoResponse
    {
        public Profile profile { get; set; }

    }

    public class Profile
    {
        public long id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string contact_name { get; set; }
        public string email { get; set; }
        public int parent_id { get; set; }
        public int level_id { get; set; }
        public int isactive { get; set; }
        public int created_by { get; set; }
        public string user_prefix { get; set; }
        public int isagency { get; set; }
        public int custom_sort_status { get; set; }
        public int merged_wallet { get; set; }
        public int special_register { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}