namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Response
{
    public class UserTransferResponse
    {
        /// <summary>
        /// AVIA系统内部转账流水号
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 商户提交的转账流水号
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        /// 本次转账的币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 转账之后会员的余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 转账之后商户的额度
        /// </summary>
        public decimal? Credit { get; set; }
    }
}
