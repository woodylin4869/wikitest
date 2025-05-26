using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Request
{
    /// <summary>
    /// 單筆交易單
    /// </summary>
    public class SingleTransactionRequest
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
        /// 交易識別唯一碼
        /// </summary>
        [MaxLength(20)]
        [Required]
        public string TransactionId { get; set; }
    }
}