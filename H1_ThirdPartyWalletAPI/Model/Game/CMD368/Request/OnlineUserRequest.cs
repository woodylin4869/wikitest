using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    /// <summary>
    /// 在線會員列表
    /// </summary>
    public class OnlineUserRequest
    {
        /// <summary>
        /// 站台代碼（20位）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PartnerKey { get; set; }
    }
}
