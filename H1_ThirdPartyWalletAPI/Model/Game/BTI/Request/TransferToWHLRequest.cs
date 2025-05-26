using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Request
{
    /// <summary>
    /// 請求 TransferToWHL 转入
    /// </summary>
    public class TransferToWHLRequest : BaseRequest
    {
        /// <summary>
        /// string(50) 玩家在 BTI 账号是唯一登入的。
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string MerchantCustomerCode { get; set; }

        /// <summary>
        /// double Amount in "major units", that is dollars, euros, pounds etc
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// string(50) Transaction's unique identification code at merchant side.
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string RefTransactionCode { get; set; }

        /// <summary>
        /// string(255) Not use, please pass empty string. 
        /// </summary>
        [Required]
        [MinLength(1)]
        [MaxLength(255)]
        public string BonusCode { get; set; }
    }
}
