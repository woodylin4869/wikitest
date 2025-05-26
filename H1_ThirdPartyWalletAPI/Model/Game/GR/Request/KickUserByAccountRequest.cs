using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0005 - 平台踢出使用者 kick_user_by_account
    /// </summary>
    public class KickUserByAccountRequest
    {
        /// <summary>
        /// 創建使用者帳號(僅能英文或數字,25 個字內)
        /// </summary>
        [MinLength(3)]
        [MaxLength(25)]
        [Required]
        public string account { get; set; }
	}
}
