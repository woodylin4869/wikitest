using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Request
{
    public class GetAcctInfoRequest : BaseRequest
    {
        /// <summary>
        /// 游戏玩家 ID
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string acctId { get; set; }
    }
}
