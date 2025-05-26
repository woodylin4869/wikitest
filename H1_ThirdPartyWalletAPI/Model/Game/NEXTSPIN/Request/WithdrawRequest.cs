using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Request
{
    public class WithdrawRequest : BaseRequest
    {
        /// <summary>
        /// 游戏玩家 ID
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string acctId { get; set; }

        /// <summary>
        /// 货币的 ISO 代码
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string currency { get; set; }

        /// <summary>
        /// 存款金额
        /// </summary>
        [Required]
        public decimal amount { get; set; }
    }
}
