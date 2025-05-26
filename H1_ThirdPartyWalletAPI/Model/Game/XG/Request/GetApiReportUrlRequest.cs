using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Request
{
    /// <summary>
    /// 取得會員下注內容統計
    /// </summary>
    public class GetApiReportUrlRequest : BaseRequest
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
        /// 幣別 幣別需該代理有啟用才能使用
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Currency { get; set; }

        /// <summary>
        /// API 語系, 參數名改用 lang 亦可
        ///  Available values : zh-CN, zh-TW, en-US
        ///  Default value : zh-CN
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ApiLang { get; set; }
    }
}
