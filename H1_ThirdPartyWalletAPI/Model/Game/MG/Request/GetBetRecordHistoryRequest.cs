using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Request
{
    /// <summary>
    /// Get bets details (bet by bet) 获取下注信息
    /// </summary>
    public class GetBetRecordHistoryRequest {
        /// <summary>
        /// 返回记录的最大数量，在 1 到 20000 之间
        /// </summary>
        [Required]
        public int Limit { get; set; }
        /// <summary>
        /// 为基于光标的分页启动 betUID. 如使用多个Betlogs, 方法必须放入”startingAfter” 的参数
        /// </summary>
        public string startingAfter { get; set; }
        /// <summary>
        /// 响应添加渠道参数
        /// </summary>
        public bool Channel { get; set; }
    }
}
