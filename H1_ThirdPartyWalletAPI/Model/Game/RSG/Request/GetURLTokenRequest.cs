using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    /// <summary>
    /// 取得遊戲網址(進入遊戲) 
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
        /// 會員暱稱
        /// </summary>
        [MinLength(1)]
        [MaxLength(20)]
        [Required]
        public string UserName { get; set; }
        /// <summary>
        /// 遊戲代碼(請參照代碼表)
        /// </summary>
        [Required]
        public int GameId { get; set; }
        /// <summary>
        /// 幣別代碼(請參照代碼表)
        /// </summary>
        [MinLength(2)]
        [MaxLength(5)]
        [Required]
        public string Currency { get; set; }
        /// <summary>
        /// 語系代碼(請參照代碼表)
        /// </summary>
        [StringLength(5)]
        [Required]
        public string Language { get; set; }
        /// <summary>
        /// 離開遊戲時導向特定網址
        /// ExitAction 帶空字串 ( ExitAction=”” ) 時，離開遊戲時將關閉視窗
        /// </summary>
        [MinLength(0)]
        [MaxLength(255)]
        public string ExitAction { get; set; }
    }
}