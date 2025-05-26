using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0003-v3 - 平台使用者轉出點數 debit_balance_v3
    /// </summary>
    public class DebitBalanceV3Request
    {
        /// <summary>
        /// 使用者帳號需包含後綴碼 {account}@{site_code}
        /// </summary>
        [MinLength(3)]
        [MaxLength(25)]
        [Required]
        public string account { get; set; }

        /// <summary>
        /// 轉出點數(無條件捨去到小數點第二位)
        /// </summary>
        [Required]
        public decimal debit_amount { get; set; }

        /// <summary>
        /// 自定義單號, 長度不超過 50 個字
        /// </summary>
        [MinLength(3)]
        [MaxLength(50)]
        [Required]
        public string order_id { get; set; }
    }
}
