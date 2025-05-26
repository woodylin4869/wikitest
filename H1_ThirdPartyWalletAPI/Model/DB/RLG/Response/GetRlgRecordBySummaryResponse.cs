using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.RLG.Response
{
    public class GetRlgRecordBySummaryResponse : RLGRecordPrimaryKey
    {
        public string gamecode { get; set; }
        public decimal totalamount { get; set; }
        public decimal bettingbalance { get; set; }
        public int status { get; set; }
        public new DateTime? drawtime { get; set; }
        public string numberofperiod { get; set; }
        public string odds { get; set; }
        public string gameplaycode { get; set; }
        public string contentcode { get; set; }
    }
}
