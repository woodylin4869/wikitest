using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Request
{
    public class BaseRequest
    {
        /// <summary>
        /// 玩家编码不能超过 50 个字符。请只使用数字、英文字母、连字符号(-) 和 下划线()
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string PlayerId { get; set; }
    }
}
