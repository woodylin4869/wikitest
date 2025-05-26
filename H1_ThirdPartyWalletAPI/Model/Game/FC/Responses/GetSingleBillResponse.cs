
using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Response
{
    public class GetSingleBillResponse : FCBaseStatusRespones
    {
        public long bankID { get; set; }
        public string trsID { get; set; }
        public string action { get; set; }
        public decimal points { get; set; }
        public string account { get; set; }
        public string currency { get; set; }
        public string eventID { get; set; }
        public int status { get; set; }
        public decimal beforepoints { get; set; }
        public decimal afterpoints { get; set; }
        public DateTime cdate { get; set; }
        public DateTime bdate { get; set; }
    }
}
