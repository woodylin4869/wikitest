namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class LotteryBetRecord : JDBBetRecordBase {
        public LotteryBetRecord()
        {
        }

        public LotteryBetRecord(CommonBetRecord betRecord) : base(betRecord)
        {
            this.hasBonusGame = betRecord.hasBonusGame;
        }

        public int hasBonusGame { get; set; }
    }

}