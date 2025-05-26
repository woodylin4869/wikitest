using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Request
{
    /// <summary>
    /// 取得單筆轉帳資料
    /// </summary>
    public class CheckTransferRequest : BaseRequest
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
        /// 交易編號，需全域唯一(global unique)，限英數字，長度 4 ~ 40 字
        /// </summary>
        [MinLength(4)]
        [MaxLength(40)]
        [Required]
        public string TransactionId { get; set; }

        /// <summary>
        /// API 語系, 參數名改用 lang 亦可
        ///  Available values : zh-CN, zh-TW, en-US
        ///  Default value : zh-CN
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ApiLang { get; set; }
    }
}
