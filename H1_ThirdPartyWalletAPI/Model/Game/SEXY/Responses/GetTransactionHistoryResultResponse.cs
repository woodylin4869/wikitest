using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response
{
    public class GetTransactionHistoryResultResponse : SEXYBaseStatusRespones
    {
        public string url { get; set; }
        public string txnUrl { get; set; }
        public string roundUrl { get; set; }
    }
}
