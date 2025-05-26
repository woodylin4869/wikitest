using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Request
{
    /// <summary>
    /// 取得遊戲每日統計資訊
    /// </summary>
    public class GetGameDailyRecordRequest
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
        /// 遊戲代碼
        /// </summary>
        [Required]
        public int GameId { get; set; }
        /// <summary>
        ///  查詢日期
        /// </summary>
        [Required]
        public string Date { get; set; }
    }
}