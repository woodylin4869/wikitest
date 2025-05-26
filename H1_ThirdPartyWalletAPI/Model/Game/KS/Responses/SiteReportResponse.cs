using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Response
{
    public class SiteReportResponse
    {
        public int RecordCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<SiteReportList> list { get; set; }
    }

    public class SiteReportList
    {
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 投注额
        /// </summary>
        public decimal BetMoney { get; set; }

        /// <summary>
        /// 有效投注
        /// </summary>
        public decimal BetAmount { get; set; }
        /// <summary>
        /// 盈亏
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        /// 派发的奖金
        /// </summary>
        public decimal Reward { get; set; }
        /// <summary>
        /// 订单量
        /// </summary>
        public int OrderCount { get; set; }
    }

}
