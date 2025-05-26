namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class PokerBetRecord : JDBBetRecordBase {
        public PokerBetRecord()
        {
        }

        public PokerBetRecord(CommonBetRecord betRecord) : base(betRecord)
        {
            this.tax = betRecord.tax;
            this.validBet = betRecord.validBet;
        }

        public decimal tax { get; set; }

        public decimal validBet { get; set; }
    }

}