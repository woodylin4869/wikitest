using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Request
{
    /// <summary>
    /// 取得 URL Token 
    /// </summary>
    public class GetURLTokenRequest
    {
        /// <summary>
        /// 系統代碼(只限英數)
        /// </summary>
        [MinLength(2)]
        [MaxLength(20)]
        [Required]
        public string SystemCode { get; set; }
        /// <summary>
        /// 站台代碼(只限英數)
        /// </summary>
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string WebId { get; set; }
        /// <summary>
        /// 會員惟一識別碼(只限英數)
        /// </summary>
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// 語系代碼(請參照代碼表)
        /// </summary>
        [StringLength(5)]
        [Required]
        public string Language { get; set; }

        /// <summary>
        /// 是否進手機板
        /// </summary>
        [Required]
        public string IsMobile { get; set; }

        /// <summary>
        /// 離開遊戲時導向特定網址
        /// ExitAction 帶空字串 ( ExitAction=”” ) 時，離開遊戲時將關閉視窗
        /// </summary>
        [MinLength(0)]
        [MaxLength(255)]
        public string ExitAction { get; set; }

        /// <summary>
        /// 直接進桌遊戲代號,詳見 I.3 彩別代號
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EnterGame { get; set; }

        /// <summary>
        /// 直接進遊戲類別代號,詳見 I.5 彩別群組代號
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EnterType { get; set; }
    }
}
