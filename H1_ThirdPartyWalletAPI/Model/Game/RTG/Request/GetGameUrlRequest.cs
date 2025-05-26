using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Request
{
    /// <summary>
    /// 取得遊戲連結 
    /// </summary>
    public class GetGameUrlRequest
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
        /// <summary>
        /// 玩家的唯一識別碼 **最多20碼
        /// </summary>
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// 遊戲ID
        /// </summary>
        [Required]
        public int GameId { get; set; }
        /// <summary>
        /// 語系
        /// </summary>
        [Required]
        public string Language { get; set; }
        /// <summary>
        /// 玩家離開時，返回網址
        /// </summary>
        public string BackUrl { get; set; }
    }
}