using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    /// <summary>
    /// 取得遊戲每分鐘統計資訊
    /// </summary>
    public class GetGameMinDetailURLTokenRequest
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
        /// 會員惟一識別碼(只限英數)
        /// </summary>
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// 幣別代碼(請參照代碼表)
        /// </summary>
        [MinLength(2)]
        [MaxLength(5)]
        [Required]
        public string Currency { get; set; }
        /// <summary>
        /// 遊戲類型(1.老虎機 2.捕魚機)
        /// </summary>
        [Required]
        public int GameType { get; set; }
        /// <summary>
        /// 遊戲代碼(請參照代碼表
        /// </summary>
        [Required]
        public int GameId { get; set; }
        /// <summary>
        /// 時間(yyyy-MM-dd HH:mm)
        /// </summary>
        [StringLength(16)]
        [Required]
        public string Time { get; set; }
        /// <summary>
        /// 語系代碼(請參照代碼表
        /// </summary>
        [MinLength(5)]
        [MaxLength(5)]
        [Required]
        public string Language { get; set; }
    }
}