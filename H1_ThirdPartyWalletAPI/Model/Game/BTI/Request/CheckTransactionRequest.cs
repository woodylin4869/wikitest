using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Request
{
    /// <summary>
    /// 請求 CheckTransaction 检查转账记录
    /// </summary>
    public class CheckTransactionRequest : BaseRequest
    {
        /// <summary>
        /// 转账码必须是唯一值不可重复 string(50)
        /// </summary>
        [Required]
        [MaxLength(50)]
        [MinLength(2)]
        public string RefTransactionCode { get; set; }
    }
}
