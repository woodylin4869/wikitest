namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Request
{
    public class GetTransactionRequest : AERequestBase
    {
        public override string action => "get_transaction";
        /// <summary>
        /// deposit or withdraw
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// transactionId
        /// </summary>
        public string tx_id { get; set; }
    }
}
