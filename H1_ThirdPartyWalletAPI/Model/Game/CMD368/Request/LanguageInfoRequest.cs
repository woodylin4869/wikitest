using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    public class LanguageInfoRequest
    {
        /// <summary>
        /// 站台代碼（20位）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PartnerKey { get; set; }
        /// <summary>
        /// 比賽代碼 0 = 隊名 ， 1 = 聯賽名 ， 2 = 特別投注名
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 聯賽ID查詢
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string ID { get; set; }
    }
}
