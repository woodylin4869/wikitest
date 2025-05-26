namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class SlotBetRecord:JDBBetRecordBase {
        public SlotBetRecord()
        {
        }

        public SlotBetRecord(CommonBetRecord betRecord) : base(betRecord)
        {
            this.gambleBet = betRecord.gambleBet;
            this.jackpot = betRecord.jackpot;
            this.jackpotContribute = betRecord.jackpotContribute;
            this.hasFreegame = betRecord.hasFreegame;
            this.hasGamble = betRecord.hasGamble;
            this.systemTakeWin = betRecord.systemTakeWin;
        }

        public decimal gambleBet { get; set; }
        public decimal jackpot { get; set; }
        public decimal jackpotContribute { get; set; }
        public int hasFreegame { get; set; }
        public int hasGamble { get; set; }
        public int systemTakeWin { get; set; }
    }

}