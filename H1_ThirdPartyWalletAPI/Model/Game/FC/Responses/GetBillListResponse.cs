using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Response
{
    public class GetBillListResponse : FCBaseStatusRespones
    {
        public List<Bank> Bank { get; set; }
    }

    public class Bank
    {
        public long bankID { get; set; }
        public string trsID { get; set; }
        public string account { get; set; }
        public decimal points { get; set; }
        public string currency { get; set; }
        public string eventID { get; set; }
        public decimal after { get; set; }
        public decimal before { get; set; }
        public DateTime createDateTime { get; set; }
    }
}
