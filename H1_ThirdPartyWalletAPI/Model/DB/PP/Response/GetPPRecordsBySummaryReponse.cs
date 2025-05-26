using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.PP.Response
{
    public class GetPPRecordsBySummaryReponse: PPRecordPrimaryKey
    {
        public string GameID { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Bet { get; set; }
        public decimal Win { get; set; }

        public string Currency { get; set; }
        public decimal Jackpot { get; set; }

        public string ExtPlayerID { get; set; }
    }
}
