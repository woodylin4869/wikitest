using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.PME.Request
{
    public class GetBalanceRequest : BaseRequest
    {
        /// <summary>
        /// 用户名（2-32位）
        /// </summary>
        [Required]
        [MaxLength(32)]
        [MinLength(2)]
        public string username { get; set; }
    }
}
