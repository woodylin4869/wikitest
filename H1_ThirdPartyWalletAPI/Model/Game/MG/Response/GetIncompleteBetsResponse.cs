using H1_ThirdPartyWalletAPI.Model.Game.MG.Enum;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Response
{
    public class GetIncompleteBetsResponse
    {
        public List<ProductIncompleteBet> IncompleteBet { get; set; }
    }

    public class ProductIncompleteBet
    {
        /// <summary>
        /// 产品编号
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// 产品返回状态
        /// </summary>
        public ProductResponseStatus Status { get; set; }

        public List<IncompleteBet> OpenBets { get; set; }
    }

    public class IncompleteBet
    {
        /// <summary>
        /// 玩家编码
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 游戏代码
        /// </summary>
        public string GameCode { get; set; }

        /// <summary>
        /// 平台 
        /// 请注意，平台参数仅与老虎机游戏有关，对于所有其他游戏，将返回“未知
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal BetAmount { get; set; }

        /// <summary>
        /// 彩金金额(派彩金額)
        /// </summary>
        public decimal PayoutAmount { get; set; }

        /// <summary>
        /// 未结束下注的 UTC 准确时间
        /// ex: 2017-04-04T12:00:51.246
        /// </summary>
        public string CreatedDateUtc { get; set; }

        /// <summary>
        /// 未结束下注结算的 UTC 大概时间
        /// ex: 2017-04-05T12:00:51.246
        /// </summary>
        public string EstimatedSettlementTimeUtc { get; set; }

        /// <summary>
        /// Playcheck 中所见的 Bet ID
        /// </summary>
        public string ExternalTransactionId { get; set; }
    }

    public class MetaData
    {
        /// <summary>
        /// 关于下注的详细信息
        /// 请注意-仅会回传虚拟体育 (Virtual Sports) 游戏的数据
        /// </summary>
        public string Description { get; set; }
    }
}
