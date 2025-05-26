using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Request
{
    /// <summary>
    /// 轉帳
    /// </summary>
    public class TransferRequest : BaseRequest
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
        /// 轉帳金額，需大於 0，可到小數點後兩位
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// 交易編號，需全域唯一(global unique)，限英數字，長度 4 ~ 40 字
        /// </summary>
        [MinLength(4)]
        [MaxLength(40)]
        [Required]
        public string TransactionId { get; set; }

        /// <summary>
        /// 轉帳類型，1 = 轉出，2 = 轉入
        /// </summary>
        [Required]
        public int TransferType { get; set; }

        /// <summary>
        /// API 語系, 參數名改用 lang 亦可
        ///  Available values : zh-CN, zh-TW, en-US
        ///  Default value : zh-CN
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ApiLang { get; set; }
    }
}
