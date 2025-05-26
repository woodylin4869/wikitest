using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Request
{
    /// <summary>
    /// 取出點數
    /// </summary>
    public class WithdrawRequest
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        [Required]
        public string SystemCode { get; set; }
        /// <summary>
        /// 站台代碼(只限英數)
        /// </summary>
        [Required]
        public string WebId { get; set; }
        /// <summary>
        /// 玩家的唯一識別碼
        /// </summary>
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// 交易識別唯一碼**需傳入不重複ID，限英數字，最長20碼
        /// </summary>
        [MaxLength(20)]
        [Required]
        public string TransactionId { get; set; }
        /// <summary>
        /// 存款金額
        /// </summary>
        [Required]
        public decimal Balance { get; set; }
    }
}