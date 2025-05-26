using H1_ThirdPartyWalletAPI.Model.Game.MG.Enum;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Request
{
    /// <summary>
    /// Create transaction 创建资金交易
    /// </summary>
    public class PostTransactionRequest
    {
        /// <summary>
        /// 玩家编码不能超过 50 个字符。请只使用数字、英文字母、连字符号 (-) 和 下划线(\_)
        /// </summary>
        [MaxLength(50)]
        public string PlayerId { get; set; }

        /// <summary>
        /// 交易种类
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// 交易金额。金额只能在取款时为空值（比如全部取出）
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 代理人交易幂等密钥 idempotencyKey, 不能超过 50 个字符 (GUID)
        /// </summary>
        [MaxLength(50)]
        public string IdempotencyKey { get; set; }

        /// <summary>
        /// 代理人交易编号. 如果没有带入idempotencyKey， 系统会把externalTransactionId视为 idempotencyKey
        /// </summary>
        public string ExternalTransactionId { get; set; }


    }
}
