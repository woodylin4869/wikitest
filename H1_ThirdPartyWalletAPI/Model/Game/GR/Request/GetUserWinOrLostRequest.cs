using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0010 – 平台取得使用者輸贏金額 get_user_win_or_lost
    /// </summary>
    public class GetUserWinOrLostRequest
    {
        /// <summary>
        /// 使用者帳號需包含後綴碼 {account}@{site_code}
        /// </summary>
        [MinLength(3)]
        [MaxLength(25)]
        [Required]
        public string account { get; set; }

        /// <summary>
        /// 遊戲代碼
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? game_type { get; set; }

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
    }
}
