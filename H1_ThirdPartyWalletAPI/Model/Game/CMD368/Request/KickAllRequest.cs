using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    /// <summary>
    /// 踢除所有會員
    /// </summary>
    public class KickAllRequest
    {
        /// <summary>
        /// 站台代碼（20位）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PartnerKey { get; set; }
        /// <summary>
        /// 會員名稱（20位）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string UserName { get; set; }
    }
}
