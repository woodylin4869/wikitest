namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Request
{
    public class DepositRequest : AERequestBase
    {
        public override string action => "deposit";

        public string account_name { get; set; }
        /// <summary>
        /// amount
        /// 有可能要轉 string
        /// </summary>
        public decimal amount { get; set; }
        /// <summary>
        /// tx_id : transactionId
        /// </summary>
        public string tx_id { get; set; }
    }
}
