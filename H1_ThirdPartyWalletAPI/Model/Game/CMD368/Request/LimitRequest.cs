using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    public class LimitRequest
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
        /// <summary>
        /// 模板名称
        /// </summary>
        [MaxLength(20)]
        [Required]
        public string TemplateName { get; set; }
    }
}
