using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    /// <summary>
    /// 取得遊戲帳務
    /// </summary>
    public class GetSummaryByTxTimeHourRequest : SexyRequestBase
    {
        /// <summary>
        /// 查询时间，使用 ISO 8601 格式 yyyy-MM-ddThh+|-hh:mm Example 范例：2021-03-26T12+08:00
        /// </summary>
        public string startTime { get; set; }
        /// <summary>
        /// 查询时间，使用 ISO 8601 格式 yyyy-MM-ddThh+|-hh:mm Example 范例：2021-03-26T12+08:00
        /// </summary>
        public string endTime { get; set; }
        public string platform { get; set; }
        //public string gameType { get; set; }
        //public string gameCode { get; set; }

    }
}