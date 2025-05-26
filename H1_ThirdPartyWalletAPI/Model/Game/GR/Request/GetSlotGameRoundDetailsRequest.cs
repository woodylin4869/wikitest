using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0007-2 - 平台取得 Slot 下注遊戲後詳細資訊的結果 get_slot_game_round_details
    /// </summary>
    public class GetSlotGameRoundDetailsRequest
    {
        /// <summary>
        /// 遊戲代碼對應表
        /// </summary>
        [Required]
        public int game_type { get; set; }

        /// <summary>
        /// 遊戲局號, 在遊戲及代理後台是顯示十六進制。
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? game_round { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? start_time { get; set; }

        /// <summary>
        /// 結束時間
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? end_time { get; set; }

        /// <summary>
        /// 目前所在分頁
        /// </summary>
        [Required]
        public int page_index { get; set; }

        /// <summary>
        /// 每頁筆數(每頁筆數{page_size}限制在 10~1000 中。)
        /// </summary>
        [Required]
        [Range(10, 100)]
        public int page_size { get; set; }
    }
}
