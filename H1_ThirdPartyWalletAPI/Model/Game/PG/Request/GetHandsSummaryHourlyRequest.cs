using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Request
{
    /// <summary>
    /// 获取每小时投注汇总
    /// </summary>
    public class GetHandsSummaryHourlyRequest
    {
        /// <summary>
        /// 运营商的唯一标识符
        /// </summary>
        public string operator_token { get; set; }
        /// <summary>
        /// PGSoft 和运营商之间的共享密码
        /// </summary>
        public string secret_key { get; set; }
        /// <summary>
        /// 投注记录开始时间范围（以毫秒为单位的 Unix 时间戳）
        /// 注： 值度为 1 - 40 天
        /// </summary>
        public long from_time { get; set; }
        /// <summary>
        /// 投注记录结束时间范围（以毫秒为单位的 Unix 时间戳）
        /// 注： 值度为 1 - 40 天
        /// </summary>
        public long to_time { get; set; }
        /// <summary>
        /// 记录货币
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// 交易类别：
        /// 1: 现金
        /// 2: 红利
        /// 3: 免费游戏
        /// </summary>
        public List<int> transaction_types { get; set; }
    }
}