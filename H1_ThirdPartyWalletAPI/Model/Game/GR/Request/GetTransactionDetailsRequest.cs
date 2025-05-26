using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0008 – 平台取得交易詳細記錄 get_transaction_details
    /// </summary>
    public class GetTransactionDetailsRequest
    {
        /// <summary>
        /// 使用者帳號需包含後綴碼 {account}@{site_code}
        /// </summary>
        [MinLength(3)]
        [MaxLength(25)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? account { get; set; }

        /// <summary>
        /// 自定義單號, 長度不超過 50 個字
        /// </summary>
        [MinLength(3)]
        [MaxLength(50)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? order_id { get; set; }

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
