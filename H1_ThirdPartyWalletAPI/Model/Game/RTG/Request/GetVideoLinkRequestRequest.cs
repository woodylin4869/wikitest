using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Request
{
    /// <summary>
    /// 取得調閱連結
    /// </summary>
    public class GetVideoLinkRequest
    {
		/// <summary>
        /// 系統代碼(只限英數
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
        /// 帳務ID
        /// </summary>
        [Required]
        public long RecordId { get; set; }
        /// <summary>
        /// 語系代碼(請參照代碼表)
        /// </summary>
        [Required]
        public string Language { get; set; }
	}
}