namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0023 - 平台取每日報表
    /// </summary>
    public class GetreportResponse : GRResponseBase
    {
        public Data data { get; set; }

        public class Data
        {
            public Bet_Report[] bet_report { get; set; }
            public int page_index { get; set; }
            public int page_size { get; set; }
            public int total_pages { get; set; }
            public int total_elements { get; set; }
        }

        public class Bet_Report
        {
            public string account { get; set; }
            public int game_type { get; set; }
            public int game_module_type { get; set; }
            public int total_bet_count { get; set; }
            public float total_bet { get; set; }
            public float total_valid_bet { get; set; }
            public float total_win { get; set; }
            public string c_type { get; set; }
            public string bet_date { get; set; }
        }

    }
}
