using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    /// <summary>
    /// 取得遊戲詳細資訊
    /// </summary>
    public class GetPagedGameDetailRequest
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
        [MinLength(0)]
        [MaxLength(20)]
        [Required]
        public string WebId { get; set; }
        /// <summary>
        /// 遊戲類型(1.老虎機 2.捕魚機)
        /// </summary>
        [Required]
        public int GameType { get; set; }
        /// <summary>
        /// 開始時間(yyyy-MM-dd HH:mm)
        /// </summary>
        [StringLength(16)]
        [Required]
        public string TimeStart { get; set; }
        /// <summary>
        /// 結束時間(yyyy-MM-dd HH:mm)
        /// </summary>
        [StringLength(16)]
        [Required]
        public string TimeEnd { get; set; }
        /// <summary>
        /// 指定目前頁數(從1開始)
        /// </summary>
        [Required]
        public int Page { get; set; }
        /// <summary>
        /// 每頁筆數(範圍：5000~20000)
        /// </summary>
        [Required]
        public int Rows { get; set; }
    }
}