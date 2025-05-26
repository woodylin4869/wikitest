using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Request
{
    /// <summary>
    /// 注單編號查詢會員下注內容
    /// </summary>
    public class GetGameDetailUrlRequest : BaseRequest
    {
        /// <summary>
        /// 公鑰(代理編號)
        /// </summary>
        [Required]
        public string AgentId { get; set; }

        /// <summary>
        /// 注單編號(等於 BetFormId)
        /// </summary>
        [Required]
        public string WagersId { get; set; }

        /// <summary>
        /// API 語系, 參數名改用 lang 亦可
        ///  Available values : zh-CN, zh-TW, en-US
        ///  Default value : zh-CN
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ApiLang { get; set; }
    }
}
