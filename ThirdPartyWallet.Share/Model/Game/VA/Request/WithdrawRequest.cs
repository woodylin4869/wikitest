using System.ComponentModel.DataAnnotations;

namespace ThirdPartyWallet.Share.Model.Game.VA.Request
{
    public class WithdrawRequest
    {
        /// <summary>
        /// 交易代碼 (需為唯一值)
        /// </summary>
        [Required(ErrorMessage = "Transaction ID is required.")]
        [StringLength(40, MinimumLength = 4, ErrorMessage = "Transaction ID must be between 4 and 40 characters.")]
        public string TransactionId { get; set; }

        /// <summary>
        /// 渠道編號 (若無渠道，請帶入1)
        /// </summary>
        [Required(ErrorMessage = "Channel ID is required.")]
        public string ChannelId { get; set; }

        /// <summary>
        /// 會員帳號 (允許字元: a-z A-Z 0-9 _-)
        /// </summary>
        [Required(ErrorMessage = "Account is required.")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "Account must be between 3 and 32 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Account can only contain letters, digits, underscores, and hyphens.")]
        public string Account { get; set; }

        /// <summary>
        /// 幣別 (請洽詢我方商務)
        /// </summary>
        [Required(ErrorMessage = "Currency is required.")]
        public string Currency { get; set; }

        /// <summary>
        /// 轉帳金額 (允許小數點後兩位)
        /// </summary>
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0 and allow up to two decimal places.")]
        public decimal Amount { get; set; }
    }
}
