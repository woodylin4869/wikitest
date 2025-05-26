using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Request
{
    public class GetBetHistoriesRequest : AERequestBase
    {
        public override string action => "get_bet_histories";
        public DateTime from_time { get; set; }
        public DateTime to_time { get; set; }

    }
}
