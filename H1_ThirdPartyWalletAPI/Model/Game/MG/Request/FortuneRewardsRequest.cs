using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Request
{
    public class FortuneRewardsRequest
    {
            public DateTime fromDate { get; set; }
            public DateTime toDate { get; set; }
            public int utcOffset { get; set; }
            public string[] rewardTypes { get; set; }
    }
}
