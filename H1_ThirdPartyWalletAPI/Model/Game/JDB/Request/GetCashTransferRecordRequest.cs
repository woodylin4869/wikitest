namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class GetCashTransferRecordRequest : RequestBaseModel
    {
        public override int Action => 55;
        public string serialNo { get; set; }
    }
}
