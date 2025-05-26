using System.ComponentModel.DataAnnotations;

namespace ThirdPartyWallet.Share.Model.Game.VA.Request
{
    public class DepositRequest
    {
        /// <summary>
        /// 交易代碼 (需為唯一值)
        /// </summary>
        [Required]
        [StringLength(40, MinimumLength = 4)] // Length constraint: 4 to 40 characters
        public string TransactionId { get; set; }

        /// <summary>
        /// 渠道編號 (若無渠道，請帶入1)
        /// </summary>
        [Required]
        public string ChannelId { get; set; }

        /// <summary>
        /// 會員帳號 (允許字元: a-z A-Z 0-9 _-)
        /// </summary>
        [Required]
        [StringLength(32, MinimumLength = 3)] // Length constraint: 3 to 32 characters
        [RegularExpression(@"^[a-zA-Z0-9_-]+$")] // Only allow a-z, A-Z, 0-9, _, -
        public string Account { get; set; }

        /// <summary>
        /// 幣別 (請洽詢我方商務)
        /// </summary>
        [Required]
        public string Currency { get; set; }

        /// <summary>
        /// 轉帳金額 (允許小數點後兩位)
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue)] // Positive amount, at least 0.01 for the smallest valid transfer
        public decimal Amount { get; set; }
    }
}
