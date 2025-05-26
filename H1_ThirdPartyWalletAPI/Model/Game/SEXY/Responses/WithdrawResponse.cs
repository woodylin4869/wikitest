using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response
{
    public class WithdrawResponse : SEXYBaseStatusRespones
    {
        public string amount { get; set; }
        public string method { get; set; }
        public int databaseId { get; set; }
        public string currentBalance { get; set; }
        public DateTime lastModified { get; set; }
        public string txCode { get; set; }

    }
}
