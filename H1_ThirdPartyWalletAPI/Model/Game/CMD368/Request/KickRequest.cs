using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    /// <summary>
    /// 會員踢線
    /// </summary>
    public class KickRequest
    {
        /// <summary>
        /// 站台代碼
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PartnerKey { get; set; }
        /// <summary>
        /// 會員名稱最長20碼
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string UserName { get; set; }
    }
}
