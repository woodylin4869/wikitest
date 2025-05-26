using Microsoft.VisualBasic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Request
{
    public class queryMerchantGameDataRequest
    {
        public string merchantId { get; set; }

        public string data { get; set; }
    }
    public class queryMerchantGameDatarawData
    {
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string currency { get; set; }
    }
}
