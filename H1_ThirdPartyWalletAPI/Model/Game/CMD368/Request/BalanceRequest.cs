using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    /// <summary>
    /// 用戶餘額
    /// </summary>
    public class BalanceRequest
    {
        /// <summary>
        /// 站台代碼（20位）
        /// </summary>
        [Required]
        [MaxLength(20)]
        [MinLength(0)]
        public string PartnerKey { get; set; }
        /// <summary>
        /// 用户名（20位）
        /// </summary>
        [Required]
        [MaxLength(20)]
        [MinLength(0)]
        public string UserName { get; set; }
    }
}
