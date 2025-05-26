using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    /// <summary>
    /// 查詢點數
    /// </summary>
    public class GetBalanceRequest
    {
        /// <summary>
        /// 系統代碼(只限英數)
        /// </summary>
        [MinLength(2)]
        [MaxLength(20)]
        [Required]
        public string SystemCode { get; set; }
        /// <summary>
        /// 站台代碼(只限英數)
        /// </summary>
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string WebId { get; set; }
        /// <summary>
        /// 會員惟一識別碼(只限英數)
        /// </summary>
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// 幣別代碼(請參照代碼表)
        /// </summary>
        [MinLength(2)]
        [MaxLength(5)]
        [Required]
        public string Currency { get; set; }
    }
}