using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.PME.Request
{
    public class TransferRequest : BaseRequest
    {
        /// <summary>
        /// 用户名（2-32位）
        /// </summary>
        [Required]
        [MaxLength(32)]
        [MinLength(2)]
        public string username { get; set; }

        /// <summary>
        /// 转账类型
        /// 1-资金转入
        /// 2-资金转出
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 转账金额
        /// >= 0.01
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// 转账订单号
        /// 20~32位
        /// </summary>
        public string merOrderId { get; set; }

        /// <summary>
        /// 币种编码币种
        /// </summary>
        public int currency_code { get; set; }
    }
}
