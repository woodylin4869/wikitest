using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Response
{
    public class withdraw2Response
    {
        public string resultCode { get; set; }
        public decimal curBalance { get; set; }
        public DateTime date { get; set; }
        public string timeZone { get; set; }
        public string transId { get; set; }
        public string currency { get; set; }
    }
}
