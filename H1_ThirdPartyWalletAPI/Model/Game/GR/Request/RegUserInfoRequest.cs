using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0004 – 平台註冊使用者 reg_user_info
    /// </summary>
    public class RegUserInfoRequest
    {
        /// <summary>
        /// 創建使用者帳號(僅能英文或數字,25 個字內)
        /// </summary>
        [MinLength(3)]
        [MaxLength(25)]
        [Required]
        public string account { get; set; }

        /// <summary>
        /// 創建使用者名稱(3~25 個字內)
        /// </summary>
        [MinLength(3)]
        [MaxLength(25)]
        [Required]
        public string display_name { get; set; }

        /// <summary>
        /// 後綴碼, 代表該代理(2~5 個字僅能英文或數字), 可以透過代理後台查看
        /// </summary>
        [MinLength(2)]
        [MaxLength(5)]
        [Required]
        public string site_code { get; set; }
	}
}
