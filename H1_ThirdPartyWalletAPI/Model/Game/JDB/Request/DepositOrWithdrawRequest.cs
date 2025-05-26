namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class DepositOrWithdrawRequest : RequestBaseModel
    {
        public override int Action => 19;
        public string Uid { get; set; }

        public string SerialNo { get; set; }

        /// <summary>
        /// allCashOutFlag 
        /// 0 not withdraw all,1 withdarw all
        /// </summary>
        public string allCashOutFlag { get; set; }

        public decimal amount { get; set; }

        public string remark { get; set; }
    }
}
