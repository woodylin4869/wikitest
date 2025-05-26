using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Response
{
    public class FortuneRewardsResponse
    {
            public string campaignName { get; set; }
            public string playerId { get; set; }
            public DateTime creditDate { get; set; }
            public string transactionId { get; set; }
            public decimal rewardAmount { get; set; }
            public decimal payout { get; set; }
            public string referenceTransactionId { get; set; }
            public string rewardType { get; set; }
    }
}
