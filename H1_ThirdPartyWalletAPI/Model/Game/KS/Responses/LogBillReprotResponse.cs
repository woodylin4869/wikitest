using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Response
{
    public class LogBillReprotResponse
    {
        public int RecordCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<LogBillReprotList> list { get; set; }
    }

    public class LogBillReprotList
    {
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

        /// <summary>
        /// 商户ID
        /// </summary>
        public int SiteID { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 条件
        /// </summary>
        public decimal Rate { get; set; }
    }

}
