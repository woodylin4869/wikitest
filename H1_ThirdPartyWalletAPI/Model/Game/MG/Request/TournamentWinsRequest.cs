using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Request
{
    public class TournamentWinsRequest
    {
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public int utcOffset { get; set; }
        public int[] tournaments { get; set; }
    }
}
