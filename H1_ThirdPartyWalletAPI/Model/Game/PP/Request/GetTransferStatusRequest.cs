namespace H1_ThirdPartyWalletAPI.Model.Game.PP.Request
{
    public class GetTransferStatusRequest
    {
        public string secureLogin { get; set; }

        public string externalTransactionId { get; set; }

        /// <summary>
        /// 會員ID
        /// </summary>
        public string externalPlayerId { get; set; }

    }
}
