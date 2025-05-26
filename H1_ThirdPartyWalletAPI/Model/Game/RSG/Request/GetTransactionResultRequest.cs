using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    /// <summary>
    /// 查詢點數交易結果
    /// </summary>
    public class GetTransactionResultRequest
    {
        /// <summary>
        /// 系統代碼(只限英數)
        /// </summary>
        [MinLength(2)]
        [MaxLength(20)]
        [Required]
        public string SystemCode { get; set; }
        /// <summary>
        /// 交易惟一識別碼(只限英數)
        /// </summary>
        [MinLength(8)]
        [MaxLength(20)]
        [Required]
        public string TransactionID { get; set; }
    }
}