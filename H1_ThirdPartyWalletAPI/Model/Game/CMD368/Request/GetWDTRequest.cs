using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    /// <summary>
    /// 存取款單交易狀態
    /// </summary>
    public class GetWDTRequest
    {
        /// <summary>
        /// 站台代碼
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PartnerKey { get; set; }
        /// <summary>
        /// 會員名稱最長20碼
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string UserName { get; set; }
        /// <summary>
        /// 合作商交一唯一單據號
        /// </summary>
        [MaxLength(50)]
        [Required]
        public string TicketNo { get; set; }
    }
}
