using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Request
{
    /// <summary>
    /// 設定會員限注
    /// </summary>
    public class SetTemplateRequest : BaseRequest
    {
        /// <summary>
        /// 公鑰(代理編號)
        /// </summary>
        [Required]
        public string AgentId { get; set; }

        /// <summary>
        /// 會員帳號，限英數字及_線，長度4~30字
        /// </summary>
        [MinLength(4)]
        [MaxLength(30)]
        [Required]
        public string Account { get; set; }

        /// <summary>
        /// 限紅 ID，多個用逗號隔，可從後台遊戲設定或 GetAgentTemplate api 取得限紅 ID
        /// example: 51,52,53
        /// </summary>
        public string? Template { get; set; }

        /// <summary>
        /// API 語系, 參數名改用 lang 亦可
        ///  Available values : zh-CN, zh-TW, en-US
        ///  Default value : zh-CN
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ApiLang { get; set; }
    }
}
