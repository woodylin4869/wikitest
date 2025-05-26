namespace H1_ThirdPartyWalletAPI.Model.Game.PME.Request
{
    public class TicketOrderQueryRequest : BaseRequest
    {
        /// <summary>
        /// 指定拉取注单的时间范围-起始时间
        /// 注：时间为北京时间，精确到秒
        /// </summary>
        public long start_time { get; set; }

        /// <summary>
        /// 指定拉取注单的时间范围-结束时间
        /// 注：时间为北京时间，精确到秒
        /// </summary>
        public long end_time { get; set;}

        /// <summary>
        /// 与page_size一起使用，表示从last_order_id开始拉取page_size条数据
        /// 值为上次拉取返回的last_order_id
        /// 数据拉取完时，last_order_id返回0
        /// 第一次拉取时可传0
        /// </summary>
        public long last_order_id { get; set; }

        /// <summary>
        /// 页数量
        /// 1000-10000
        /// </summary>
        public int page_size { get; set; }

        /// <summary>
        /// 总商户：true，站点商户：false
        /// </summary>
        public bool agency { get; set; }

        /// <summary>
        /// 币种编码
        /// </summary>
        public int currency_code { get; set; }
    }
}
