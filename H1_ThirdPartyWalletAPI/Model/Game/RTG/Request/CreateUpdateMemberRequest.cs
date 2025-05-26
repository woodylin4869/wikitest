using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Request
{
    /// <summary>
    /// 建立與更新會員
    /// </summary>
    public class CreateUpdateMemberRequest
    {
		/// <summary>
        /// 系統代碼
        /// </summary>
        [Required]
        public string SystemCode { get; set; }
        /// <summary>
        /// 站台代碼(只限英數)
        /// </summary>
        [Required]
        public string WebId { get; set; }
        /// <summary>
        /// 玩家的唯一識別碼 **最多20碼
        /// </summary>
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string UserId { get; set; }
        /// <summary>
        /// 玩家暱稱 **最多20碼
        /// </summary>
        [MinLength(3)]
        [MaxLength(20)]
        [Required]
        public string UserName { get; set; }
        /// <summary>
        /// 幣別代碼(請參照代碼表)
        /// </summary>
        [Required]
        public string Currency { get; set; }
	}
}

