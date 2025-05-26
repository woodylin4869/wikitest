namespace H1_ThirdPartyWalletAPI.Model.Game.PME.Request
{
    public class TransferQueryRequest : BaseRequest
    {
        /// <summary>
        /// 转账订单号
        /// 20~32位
        /// </summary>
        public string merOrderId { get; set; }
    }
}
