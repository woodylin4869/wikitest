using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0014 – 平台使用者取得餘額 get_balance
    /// </summary>
    public class GetBalanceRequest
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
