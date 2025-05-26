using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    /// <summary>
    /// 踢除所有會員
    /// </summary>
    public class KickAllResponse
    {
        /// <summary>
        /// PartnerKey字碼最長20碼
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PartnerKey { get; set; }
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int Code { get; set; }
    }
}
