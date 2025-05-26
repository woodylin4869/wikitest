using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    /// <summary>
    /// 創建用戶
    /// </summary>
    public class GetCreateUserRequest
    {
        /// <summary>
        /// 用户名（1-20位）
        /// </summary>
        [Required]
        [MaxLength(20)]
        [MinLength(1)]
        public string Username { get; set; }
        /// <summary>
        /// 站台代碼（20位）
        /// </summary>
        [Required]
        [MaxLength(20)]
        [MinLength(1)]

        public string Partnerkey { get; set; }
        /// <summary>
        /// 貨幣代碼
        /// </summary>
        [Required]
        [MaxLength(5)]
        [MinLength(1)]

        public string CurrencyCode { get; set; }
    }
}
