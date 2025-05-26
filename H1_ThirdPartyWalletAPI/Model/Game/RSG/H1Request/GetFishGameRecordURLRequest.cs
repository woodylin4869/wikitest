using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    public class GetFishGameRecordURLRequest
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
        [MinLength(0)]
        [MaxLength(20)]
        [Required]
        public string WebId { get; set; }
        /// <summary>
        ///會員惟一識別碼(只限英數
        /// </summary>
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string UserId { get; set; }
        /// <summary>
        ///SessionId
        /// </summary>
        [MinLength(18)]
        [MaxLength(20)]
        [Required]
        public string SessionId { get; set; }
        /// <summary>
        ///Time
        /// </summary>
        [Required]
        public string Time { get; set; }
        /// <summary>
        /// 結束時間(yyyy-MM-dd HH:mm)
        /// </summary>
        [StringLength(5)]
        [Required]
        public string Language { get; set; }
    }
}
