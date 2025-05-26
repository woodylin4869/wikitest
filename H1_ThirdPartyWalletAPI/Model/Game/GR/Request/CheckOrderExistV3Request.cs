using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0018 – 平台檢查是否已有單號存在 check_order_exist_v3
    /// </summary>
    public class CheckOrderExistV3Request
    {
        /// <summary>
        /// 使用者帳號需包含後綴碼 {account}@{site_code}
        /// </summary>
        [MinLength(3)]
        [MaxLength(25)]
        [Required]
        public string account { get; set; }

        /// <summary>
        /// 自定義單號, 長度不超過 50 個字
        /// </summary>
        [MinLength(3)]
        [MaxLength(50)]
        [Required]
        public string order_id { get; set; }
    }
}
