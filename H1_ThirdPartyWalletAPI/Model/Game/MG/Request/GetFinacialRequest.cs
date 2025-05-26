using H1_ThirdPartyWalletAPI.Model.Game.MG.Enum;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Request
{
    /// <summary>
    /// Get detailed financial report 获取详细资金报表
    /// </summary>
    public class GetFinacialRequest{
        /// <summary>
        /// 在搜索条件中使用的日期/时间。 搜索条件定义为起始日期 FromDate 大于或等于 ＜搜寻日期/时间＞ 并且小于截止日期 ToDate e.g. 2023-04-28 00:00:00
        /// </summary>
        [Required]
        public string fromDate { get; set; }
        /// <summary>
        /// 在搜索条件中使用的日期/时间。 搜索条件定义为起始日期 FromDate 大于或等于 ＜搜寻日期/时间＞ 并且小于截止日期 ToDate e.g. 2023-04-28 00:00:00
        /// </summary>
        [Required]
        public string toDate { get; set; }
        /// <summary>
        /// ALLOWED:Monthly, Daily, Hourly
        /// </summary>
        [Required]
        public TimeAggregation timeAggregation { get; set; }
        /// <summary>
        /// UTC 时间差
        /// </summary>
        [Required]
        public int utcOffset { get; set; }
        /// <summary>
        /// 货币码 [ISO 4217](https://en.wikipedia.org/wiki/ISO_4217)
        /// </summary>
        [Required]
        public string Currency { get; set; }
        //public List<string> Products { get; set; }
    }
}
