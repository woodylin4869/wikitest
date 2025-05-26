using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Response
{
    public class queryMerchantGameDataResponse
    {

        public string resultCode { get; set; }
        public List<TransList> transList { get; set; }


        public class TransList
        {
            public string merchantId { get; set; }
            public string gameDate { get; set; }
            public decimal betAmount { get; set; }
            public decimal winAmount { get; set; }
            public decimal commissionable { get; set; }
            public int numberOfGames { get; set; }
            public string roomFee { get; set; }
            public string profit { get; set; }
            public string timeZone { get; set; }
            public string currency { get; set; }
        }
    }
}
