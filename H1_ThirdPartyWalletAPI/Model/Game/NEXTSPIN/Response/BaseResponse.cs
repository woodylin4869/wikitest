using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response
{
    public class BaseResponse
    {
        /// <summary>
        /// 用于标识消息的序列, 由调用者生成
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string serialNo { get; set; }

        /// <summary>
        /// 处理结果代码定义 code
        /// </summary>
        [Required]
        public int code { get; set; }

        /// <summary>
        /// 处理结果描述
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string msg { get; set; }

        /// <summary>
        /// 标识商户 ID
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string merchantCode { get; set; }

        public bool IsSuccess => code == (int)NEXTSPIN.ErrorCode.Success;
    }
}
