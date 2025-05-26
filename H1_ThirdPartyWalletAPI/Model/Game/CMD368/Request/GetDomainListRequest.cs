using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    public class GetDomainListRequest
    {
        /// <summary>
        /// 唯一生成標誌
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Token { get; set; }
        /// <summary>
        /// 語言代碼
        /// </summary>
        [Required]
        [MaxLength(5)]
        public string LanguageCode { get; set; }
        /// <summary>
        /// 用戶名
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string UserName { get; set; }
        /// <summary>
        /// 貨幣代碼
        /// </summary>
        [Required]
        [MaxLength(5)]
        public string CurrencyCode { get; set; }
        /// <summary>
        /// 頁面模板 總共六個
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string TemplateName { get; set; }
        /// <summary>
        /// 模板風格
        /// </summary>
        [Required]
        [MaxLength(2)]
        public string View { get; set; }
    }
}
