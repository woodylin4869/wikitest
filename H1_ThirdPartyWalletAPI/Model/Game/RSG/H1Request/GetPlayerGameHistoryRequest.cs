using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    /// <summary>
    /// ( 老虎機專用 ) 取得特定 Session 內的遊戲歷程
    /// </summary>
    public class GetPlayerGameHistoryRequest
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
        /// 會員惟一識別碼(只限英數
        /// </summary>
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// Session
        /// </summary>
        [MinLength(18)]
        [MaxLength(20)]
        [Required]
        public string SessionId { get; set; }
        /// <summary>
        /// 站台代碼(只限英數)
        /// </summary>
        [Required]
        public int Page { get; set; }
        /// <summary>
        /// 站台代碼(只限英數)
        /// </summary>
        [Required]
        public int Rows { get; set; }
    }
}