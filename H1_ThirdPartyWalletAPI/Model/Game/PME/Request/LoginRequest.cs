using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.PME.Request
{
    public class LoginRequest : BaseRequest
    {
        /// <summary>
        /// 用户名（2-32位）
        /// </summary>
        [Required]
        [MaxLength(32)]
        [MinLength(2)]
        public string username { get; set; }

        /// <summary>
        /// 用户密码 （6-30位）
        /// </summary>
        [Required]
        [MaxLength(30)]
        [MinLength(6)]
        public string password { get; set; }

        /// <summary>
        /// 客户端ip
        /// 传递ip时需要将ipv4 转换为long类型的数字
        /// ex: 1736793618
        /// </summary>
        public long client_ip { get; set; }
    }
}
