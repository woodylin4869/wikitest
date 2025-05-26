using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Request
{
    /// <summary>
    /// 取得會員登入連結
    /// </summary>
    public class LoginRequest : BaseRequest
    {
        /// <summary>
        /// 公鑰(代理編號)
        /// </summary>
        [Required]
        public string AgentId { get; set; }

        /// <summary>
        /// 遊戲語系
        /// </summary>
        [Required]
        public string Lang { get; set; }

        /// <summary>
        /// 會員帳號，限英數字及_線，長度4~30字
        /// </summary>
        [MinLength(4)]
        [MaxLength(30)]
        [Required]
        public string Account { get; set; }

        /// <summary>
        /// 遊戲類別，直接入桌時使用
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? GameType { get; set; }

        /// <summary>
        /// 遊戲局桌檯Id，直接入桌時使用，GameType 為 4 (多桌)時請請勿帶此參數，GameType 對應 TableId 請參考文件主頁
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TableId { get; set; }

        /// <summary>
        /// UI 版本 Available values : 1.0, 2.0
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UIVersion { get; set; }

        /// <summary>
        /// API 語系, 參數名改用 lang 亦可
        ///  Available values : zh-CN, zh-TW, en-US
        ///  Default value : zh-CN
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ApiLang { get; set; }
    }
}
