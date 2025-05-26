namespace H1_ThirdPartyWalletAPI.Model.Game.WM.Request
{
    public class GetDateTimeReportRequest
    {
        public string cmd { get; set; }
        public string vendorId { get; set; }
        public string signature { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        /// <summary>
        /// 0:抓下注时间, 1:抓结算时间
        /// </summary>
        public int timetype { get; set; }
        /// <summary>
        /// 0:输赢报表, 1:小费报表, 2:全部
        /// </summary>
        public int datatype { get; set; }
        public int syslang { get; set; }

        public long timestamp { get; set; }
    }
}
