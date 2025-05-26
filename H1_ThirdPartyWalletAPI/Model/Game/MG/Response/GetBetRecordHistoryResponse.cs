using H1_ThirdPartyWalletAPI.Model.Game.MG.Enum;
using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Response
{
    /// <summary>
    /// Get bets details (bet by bet) 获取下注信息
    /// </summary>
    public class GetBetRecordHistoryResponse {
        public List<BetRecord> BetRecords { get; set; }
    }
    public class BetRecord
    {
        public Guid summary_id { get; set; }
        /// <summary>
        /// 独一的下注编号。 字段长度应正好是 36 个字符
        /// </summary>
        public string BetUID { get; set; }
        /// <summary>
        /// 在 MG Plus 系统创建下注的时间
        /// </summary>
        public DateTime? createdDateUTC { get; set; }
        /// <summary>
        /// 下注开始的时间
        /// </summary>
        public DateTime? gameStartTimeUTC { get; set; }

        /// <summary>
        /// 下注结束的时间
        /// </summary>
        public DateTime? gameEndTimeUTC { get; set; }
        /// <summary>
        /// 玩家编码不能超过 50 个字符。请只使用数字、英文字母、连字符号 (-) 和 下划线 (\_)
        /// </summary>
        public string PlayerId { get; set; }
        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// 个别产品上的玩家ID
        /// </summary>
        public string ProductPlayerId { get; set; }
        /// <summary>
        /// 游戏平台 (Unknown/Desktop/Mobile)
        /// </summary>
        public string Platform { get; set; }
        /// <summary>
        /// 游戏代码
        /// </summary>
        public string GameCode { get; set; }
        /// <summary>
        /// 渠道
        /// </summary>
        public string Channel { get; set; }
        /// <summary>
        /// Currency code 货币码
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal? BetAmount { get; set; }
        /// <summary>
        /// 派彩金额
        /// </summary>
        public decimal? PayoutAmount { get; set; }
        /// <summary>
        /// 投注状态
        /// </summary>
        public BetStatus BetStatus { get; set; }
        /// <summary>
        /// 彩池贡献金额
        /// </summary>
        public decimal? PCA { get; set; }
        /// <summary>
        /// Playcheck 中所见的 Bet ID
        /// </summary>
        public string ExternalTransactionId { get; set; }
        public metaData MetaData { get; set; }

        public decimal jackpotwin { get; set; }


        public DateTime report_time { get; set; }

    }
    public class metaData
    {

        public string SessionId { get; set; }
        public string TransactionId { get; set; }
        /// <summary>
        /// 关于下注的详细信息。请注意-仅会回传虚拟体育 (Virtual Sports) 游戏的数据
        /// </summary>
        public string Description { get; set; }
    }
}
