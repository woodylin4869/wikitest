using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0009 –平台取得所有在線遊戲有效投注總額 get_user_bet_amount
    /// </summary>
    public class GetUserBetAmountRequest
    {
        /// <summary>
        /// 使用者帳號需包含後綴碼 {account}@{site_code}
        /// </summary>
        [MinLength(3)]
        [MaxLength(25)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? account { get; set; }

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
