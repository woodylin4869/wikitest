using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Request
{
    /// <summary>
    /// 取得遊戲中的會員 
    /// </summary>
    public class GetOnlineUserRequest
    {
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
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string WebId { get; set; }
        /// <summary>
        /// 遊戲代碼(請參照代碼表)
        /// </summary>
        [Required]
        public int GameId { get; set; }
        /// <summary>
        /// 指定目前頁數(從 1 開始)
        /// </summary>
        [Range(1, int.MaxValue)]
        [Required]
        public int Page { get; set; }
        /// <summary>
        /// 每頁筆數(範圍：100~500)
        /// </summary>
        [Range(10, 100)]
        [Required]
        public int Rows { get; set; }
    }
}