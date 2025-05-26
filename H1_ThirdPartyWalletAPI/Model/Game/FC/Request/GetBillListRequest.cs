using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Request
{
    public class GetBillListRequest
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
