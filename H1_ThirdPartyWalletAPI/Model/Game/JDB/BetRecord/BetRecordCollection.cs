using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class BetRecordCollection
    {
        public List<SlotBetRecord> SlotBetRecords { get; set; }
        public List<FishBetRecord> FishBetRecords { get; set; }
        public List<ArcadeBetRecord> ArcadeBetRecords { get; set; }
        public List<LotteryBetRecord> LotteryBetRecords { get; set; }
        public List<PokerBetRecord> PokerBetRecords { get; set; }

    }

}