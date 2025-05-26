namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class FishBetRecord:JDBBetRecordBase {
        public FishBetRecord()
        {
        }

        public FishBetRecord(CommonBetRecord betRecord) : base(betRecord)
        {
            this.roomType = betRecord.roomType;
            this.beforeBalance = betRecord.beforeBalance;
            this.afterBalance = betRecord.afterBalance;
        }

        public int roomType { get; set; }
        public decimal beforeBalance { get; set; }

        public decimal afterBalance { get; set; }

       
    }

}