using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Request
{
    public class GetBetTotalListRequest
    {
        public string SystemCode { get; set; }
        public string WebId { get; set; }
        public string? GameId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }


    public class GetBetTotalListserverRequest
    {
        public string SystemCode { get; set; }
        public string WebId { get; set; }
        public string? GameId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
