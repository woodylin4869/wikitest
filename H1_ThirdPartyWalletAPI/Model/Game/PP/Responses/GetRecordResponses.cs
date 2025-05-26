using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.PP.Responses
{
    public class GetRecordResponses
    {
        public string PlayerID { get; set; }
        public string ExtPlayerID { get; set; }
        public string GameID { get; set; }
        public long PlaySessionID { get; set; }
        public string ParentSessionID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public decimal Bet { get; set; }
        public decimal Win { get; set; }
        public string Currency { get; set; }
        public decimal Jackpot { get; set; }
        public Guid summary_id { get; set; }
        public decimal pre_Bet { get; set; }
        public decimal pre_Win { get; set; }
        public string club_id { get; set; }
        public string franchiser_id { get; set; }

        public DateTime report_time { get; set; }
    }
}
