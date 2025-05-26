namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Response
{
    public class QueryTransbyIdResponse
    {
        public string resultCode { get; set; }
        /// <summary>
        /// 该交易订单的交易时间
        /// </summary>
        public string transId { get; set; }
        /// <summary>
        /// 该交易订单的交易时间
        /// </summary>
        public string transTime { get; set; }
        /// <summary>
        /// 该交易订单的交易类型：T05（转入）T06（转出）
        /// </summary>
        public string transType { get; set; }
        /// <summary>
        /// 该交易订单的交易金额（转换后）
        /// </summary>
        public string transCoins { get; set; }
        public string curBalance { get; set; }
        public string timeZone { get; set; }
        /// <summary>
        /// 该交易订单的交易状态：1：表示正常2：表示处理异常
        /// </summary>
        public string status { get; set; }
        public string currency { get; set; }
    }
}
