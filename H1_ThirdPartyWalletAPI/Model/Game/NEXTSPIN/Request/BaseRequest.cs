using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Request
{
    public class BaseRequest
    {
        /// <summary>
        /// 标识商户 ID
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string merchantCode { get; set; }

        /// <summary>
        /// 用于标识消息的序列, 由调用者生成
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string serialNo { get; set; }
    }
}
