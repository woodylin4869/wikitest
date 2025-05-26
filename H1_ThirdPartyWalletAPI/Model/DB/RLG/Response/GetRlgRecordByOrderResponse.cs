namespace H1_ThirdPartyWalletAPI.Model.DB.RLG.Response
{
    public class GetRlgRecordByOrderResponse : RLGRecordPrimaryKey
    {
        public decimal pre_totalamount { get; set; }
        public decimal pre_bettingbalance { get; set; }
    }
}
