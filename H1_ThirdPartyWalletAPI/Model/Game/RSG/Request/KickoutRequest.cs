using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    /// <summary>
    /// 剔除遊戲中的會員
    /// </summary>
    public class KickoutRequest
    {
        /// <summary>
        /// 剔除模式，有下列 4 種
        /// 1:System, 2:Web, 3:Game, 4:Player
        /// KickType = 1，會剔除系統下所有人，WebId、UserId 請填空字串，GameId 請填 0
        /// KickType = 2，會剔除站台下所有人，UserId 請填空字串，GameId 請填 0
        /// KickType = 3，會剔除正在該遊戲的所有人，WebId、UserId 請填空字串
        /// KickType = 4，會剔除特定會員，GameId 請填 0
        /// </summary>
        [Required]
        public int KickType { get; set; }
        /// <summary>
        /// 系統代碼(只限英數)
        /// </summary>
        [MinLength(2)]
        [MaxLength(20)]
        [Required]
        public string SystemCode { get; set; }
        /// <summary>
        /// 站台代碼(只限英數)
        /// </summary>
        [MinLength(0)]
        [MaxLength(20)]
        [Required]
        public string WebId { get; set; }
        /// <summary>
        /// 會員惟一識別碼(只限英數)
        /// </summary>
        [MinLength(0)]
        [MaxLength(20)]
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// 遊戲代碼(請參照代碼表)
        /// 若 KickType 不為 3，則填 0
        /// </summary>
        [Required]
        public int GameId { get; set; }
    }
}