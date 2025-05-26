using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Request
{
    public class GetBetRecordSummaryRequest
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        [Required]
        public int FilterAgent { get; set; }

    }
}
