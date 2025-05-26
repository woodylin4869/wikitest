using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response
{
    public class GetBalanceResponse : SEXYBaseStatusRespones
    {
        public Result[] results { get; set; }
        public int count { get; set; }
        public DateTime querytime { get; set; }

        public class Result
        {
            public string userId { get; set; }
            public string balance { get; set; }
            public DateTime lastModified { get; set; }
        }
    }
}
