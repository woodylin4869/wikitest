using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Response
{
    public class WithdrawResponse : AEResponseBase
    {
        public decimal balance { get; set; }

        public decimal end_balance { get; set; }

        public DateTime tx_time { get; set; }
    }

}
