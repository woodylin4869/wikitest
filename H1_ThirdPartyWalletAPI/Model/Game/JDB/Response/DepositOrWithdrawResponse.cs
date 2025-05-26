using System;


namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class DepositOrWithdrawResponse : ResponseBaseModel
    {
        public decimal UserBalance { get; set; }
        public decimal userCashBalance { get; set; }
        public decimal agentCashBalance { get; set; }
        public decimal amount { get; set; }
        public string serialNo { get; set; }
        public long Pid { get; set; }
        public DateTime PayDate { get; set; }
    }
}
