using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Request
{
    /// <summary>
    /// 取得遊戲帳務
    /// </summary>
    public class GameSettlementRecordRequest
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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? GameId { get; set; }
        /// <summary>
        /// 起始時間 (yyyy-MM-dd HH:mm:ss
        /// </summary>
        [Required]
        public string StartTime { get; set; }
        /// <summary>
        /// 結束時間 (yyyy-MM-dd HH:mm:ss)
        /// </summary>
        [Required]
        public string EndTime { get; set; }
        /// <summary>
        /// 指定目前頁數(從 1 開始)
        /// </summary>
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