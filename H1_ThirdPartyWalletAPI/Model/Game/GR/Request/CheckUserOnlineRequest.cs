using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0001 – 平台確認使用者是否在線上 check_user_online
    /// </summary>
    public class CheckUserOnlineRequest
    {
        /// <summary>
        /// 使用者帳號需包含後綴碼 {account}@{site_code}
        /// </summary>
        [MinLength(3)]
        [MaxLength(25)]
        [Required]
        public string account { get; set; }
	}
}
