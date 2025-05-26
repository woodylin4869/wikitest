using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Request
{
    /// <summary>
    /// 取得會員下注內容
    /// </summary>
    public class GetBetRecordByTimeRequest : BaseRequest
    {
        /// <summary>
        /// 公鑰(代理編號)
        /// </summary>
        [Required]
        public string AgentId { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        [Required]
        //[DefaultValue(null)]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 結束時間
        /// </summary>
        [Required]
        //[DefaultValue(null)]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 頁數，最小 1
        /// </summary>
        [Required]
        public int Page { get; set; }

        /// <summary>
        /// default: 10 每頁筆數，上限10000
        /// </summary>
        [Required]
        [Range(1, 10000)]
        public int PageLimit { get; set; }

        /// <summary>
        /// API 語系, 參數名改用 lang 亦可
        ///  Available values : zh-CN, zh-TW, en-US
        ///  Default value : zh-CN
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ApiLang { get; set; }
    }
}
