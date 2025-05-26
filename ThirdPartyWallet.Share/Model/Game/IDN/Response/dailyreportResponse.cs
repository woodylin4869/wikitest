namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class dailyreportResponse
    {
        public List<Daily_Report> daily_report { get; set; }
    }


    public class Daily_Report
    {
        public decimal turn_over { get; set; }
        public decimal player_wl { get; set; }
        public string agent_comm { get; set; }
        public string period { get; set; }
        public string game_username { get; set; }
        public string game_slug { get; set; }
    }


}