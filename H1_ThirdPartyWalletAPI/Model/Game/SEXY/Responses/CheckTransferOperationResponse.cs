using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response
{
    public class CheckTransferOperationResponse : SEXYBaseStatusRespones
    {
        public int txStatus { get; set; }
        public float balance { get; set; }
        public int transferAmount { get; set; }

        /// <summary>
        /// DEPOSIT  / Withdraw
        /// </summary>
        public string transferType { get; set; }
        public string txCode { get; set; }

    }
}
