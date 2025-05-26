using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Request
{
    public class AuthorizeRequest : BaseRequest
    {
        /// <summary>
        /// 游戏玩家 ID
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string acctId { get; set; }

        [Required]
        [MaxLength(128)]
        public string token { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        [MaxLength(10)]
        public string language { get; set; }

        /// <summary>
        /// 游戏代码
        /// </summary>
        [MaxLength(10)]
        public string gameCode { get; set; }

        /// <summary>
        /// 测试游戏
        /// </summary>
        public bool forFun { get; set; }
    }
}
