using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response
{
    public class KickAcctResponse : BaseResponse
    {
        /// <summary>
        /// 退出的用户列表
        /// </summary>
        [Required]
        public string[] list { get; set; }
    }
}
