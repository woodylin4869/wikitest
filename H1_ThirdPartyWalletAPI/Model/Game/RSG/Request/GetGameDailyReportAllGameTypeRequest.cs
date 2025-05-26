using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    /// <summary>
    /// 取得遊戲每日統計資訊(全部遊戲類型) 
    /// </summary>
    public class GetGameDailyReportAllGameTypeRequest
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
        /// 查詢日期(yyyy-MM-dd)
        /// </summary>
        [StringLength(10)]
        [Required]
        public string Date { get; set; }
    }
}