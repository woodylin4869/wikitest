using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Request
{
    /// <summary>
    /// 請求 TransferFromWHL 转出
    /// </summary>
    public class TransferFromWHLRequest : BaseRequest
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
        /// RefTransactionCode 是 string(50) Transaction's unique identification code at merchant side.
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string RefTransactionCode { get; set; }

    }
}
