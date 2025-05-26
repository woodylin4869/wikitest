using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Request
{
    public class KickAcctRequest : BaseRequest
    {
        /// <summary>
        /// 玩家 ID, 如果为空则退出全部
        /// </summary>
        [MaxLength(50)]
        public string acctId { get; set; }
    }
}
