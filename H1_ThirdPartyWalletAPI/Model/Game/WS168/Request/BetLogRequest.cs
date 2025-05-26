using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.WS168.Request
{
    public class BetLogRequest
    {
        public string time_type { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public int page { get; set; }
        public int page_size { get; set; }
    }
}
