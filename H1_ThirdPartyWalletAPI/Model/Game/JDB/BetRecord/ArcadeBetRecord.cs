namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class ArcadeBetRecord : JDBBetRecordBase {
        public ArcadeBetRecord()
        {
        }

        public ArcadeBetRecord(CommonBetRecord betRecord) : base(betRecord)
        {
            this.gambleBet = betRecord.gambleBet;
            this.hasBonusGame = betRecord.hasBonusGame;
            this.hasGamble = betRecord.hasGamble;
        }

        public decimal gambleBet { get; set; }
        public int hasBonusGame { get; set; }
        public int hasGamble { get; set; }
    }

}