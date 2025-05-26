using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Request
{
    /// <summary>
    /// 剔除遊戲中的會員
    /// </summary>
    public class KickAllRequest
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        [Required]
        public string SystemCode { get; set; }
        /// <summary>
        /// 站台代碼(只限英數)
        /// </summary>
        [Required]
        public string WebId { get; set; }
    }
}