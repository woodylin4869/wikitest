using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.PME.Request
{
    public class RegisterRequest : BaseRequest
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
        /// 用户类型
        /// 0-正式
        /// </summary>
        public int tester { get; set; }

        /// <summary>
        /// 币种编码
        /// </summary>
        public int currency_code { get; set; }
    }
}
