using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MP.Response
{
    public class CheckSummaryResponse
    {

       
            public string m { get; set; }
            public int s { get; set; }
            public CheckSummary d { get; set; }
        

        public class CheckSummary
        {
            public int code { get; set; }
            public List<Transaction> Transactions { get; set; }
        }

        public class Transaction
        {
            public int betCount { get; set; }
            public decimal totalBetAmount { get; set; }
            public decimal playerPL { get; set; }
            public decimal totalWinAmount { get; set; }
        }

    }
}
