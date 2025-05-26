using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    public class ParlayBetRecordRequest
    {
        /// <summary>
        /// 站台代碼（20位）
        /// </summary>
        [Required]
        [MaxLength(20)]
        [MinLength(0)]
        public string PartnerKey { get; set; }

        /// <summary>
        /// 混合过关注单 ID
        /// </summary>
        [Required]
        public int SocTransID { get; set; }
    }
}
