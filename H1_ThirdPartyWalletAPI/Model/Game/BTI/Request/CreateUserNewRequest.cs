using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Request
{
    /// <summary>
    /// 請求 CreateUserNew 创新会员 （新）
    /// </summary>
    public class CreateUserNewRequest : BaseRequest
    {
        /// <summary>
        /// string(50) 玩家账号是唯一登入的，必须是无重复(支持英文字母数字, 下划线, 划线, 电邮符号 @ 或 英文的句点)其他语言符号不支持。
        /// 这个参数是每个接口用的唯一值来锁定哪个玩家在 BTI
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string MerchantCustomerCode { get; set; }

        /// <summary>
        /// string(50) 玩家账号，不可重复 (支持英文字母数字, 下划线, 划线, 电邮符号 @ 或 英文的句点)其他语言符号不支持。
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string LoginName { get; set; }

        /// <summary>
        /// string(3) 货币必须英文. ISO 4217 标准. 如 RMB, CNY
        /// </summary>
        [Required]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// string(2) 国家必须英文 ISO 3166-1 标准. 如 CN 
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string CountryCode { get; set; }

        /// <summary>
        /// string(50) 城市，可以中文
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string City { get; set; }

        /// <summary>
        /// string(100) 姓， 可以中文
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string FirstName { get; set; }

        /// <summary>
        /// string(100) 名，可以中文
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// Int 请放 0（新玩家）或 1 （普通玩家)
        /// </summary>
        [Required]
        public int Group1ID { get; set; }

        /// <summary>
        /// string(4000) 请带参数然后留空
        /// </summary>
        [Required]
        [MinLength(1)]
        [MaxLength(4000)]
        public string CustomerMoreInfo { get; set; }

        /// <summary>
        /// string(2) 玩家语言 ISO 639-1. 标准如 zh
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(2)]
        public string CustomerDefaultLanguage { get; set; }

        /// <summary>
        /// String 请带参数然后留空
        /// </summary>
        [Required]
        public string DomainID { get; set; }

        /// <summary>
        /// String 日/月/年份– 生日日期来确定年龄过 18, 可留空
        /// </summary>
        [Required]
        public string DateOfBirth { get; set; }
    }
}
