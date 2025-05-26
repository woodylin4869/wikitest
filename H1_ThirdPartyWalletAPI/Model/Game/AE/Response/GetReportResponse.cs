namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Response
{
    public class GetReportResponse
    {
        public Winloss_Summary[] winloss_summary { get; set; }
        public class Winloss_Summary
        {
            public int site_id { get; set; }
            public string currency { get; set; }
            public int rounds { get; set; }
            public decimal bet_amt { get; set; }
            public decimal payout_amt { get; set; }
            public decimal jp_jc_con_amt { get; set; }
            public decimal jp_jc_win_amt { get; set; }
            public decimal jp_pc_win_amt { get; set; }
            public decimal jp_pc_con_amt { get; set; }
            public string account_name { get; set; }
            public int game_id { get; set; }
            public decimal net_amt { get; set; }
        }
    }
}
