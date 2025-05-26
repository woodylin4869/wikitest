using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    public class GetAPILogByTimeRequest
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
        /// 開始時間(yyyy-MM-dd HH:mm)
        /// </summary>
        [StringLength(16)]
        [Required]
        public string TimeStart { get; set; }
        /// <summary>
        /// 結束時間(yyyy-MM-dd HH:mm)
        /// </summary>
        [StringLength(16)]
        [Required]
        public string TimeEnd { get; set; }
    }
}
