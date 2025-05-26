using System.ComponentModel.DataAnnotations;

namespace ThirdPartyWallet.Share.Model.Game.VA.Response
{
    public class DepositResponse
    {
        /// <summary>
        /// 交易代碼
        /// </summary>

        public string TransactionId { get; set; }

        /// <summary>
        /// 渠道編號
        /// </summary>

        [Range(1, int.MaxValue)] // Assuming channelId is a positive number
        public int ChannelId { get; set; }

        /// <summary>
        /// 會員帳號
        /// </summary>

        [StringLength(32, MinimumLength = 3)] // Length constraint: 3 to 32 characters
        public string Account { get; set; }

        /// <summary>
        /// 轉帳金額
        /// </summary>

        [Range(0.01, double.MaxValue)] // Positive amount, at least 0.01 for the smallest valid transfer
        public decimal Amount { get; set; }

        /// <summary>
        /// 幣別 (請洽詢我方商務)
        /// </summary>

        public string Currency { get; set; }

        /// <summary>
        /// 轉帳前錢包餘額
        /// </summary>

        public decimal BalanceBefore { get; set; }

        /// <summary>
        /// 轉帳後錢包餘額
        /// </summary>

        public decimal BalanceAfter { get; set; }
    }
}
