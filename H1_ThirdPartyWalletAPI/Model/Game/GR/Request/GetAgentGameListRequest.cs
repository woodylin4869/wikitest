using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0021 – 平台取得代理遊戲列表 get_agent_game_list
    /// </summary>
    public class GetAgentGameListRequest
    {
        /// <summary>
        /// 代表遊戲種類
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? game_module_type { get; set; }

        /// <summary>
        /// 目前有 zh_CN(此為預設), zh_TW, en_US
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? language_type { get; set; }

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
