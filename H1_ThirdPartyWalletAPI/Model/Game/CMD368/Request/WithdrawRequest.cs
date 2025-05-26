using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    /// <summary>
    /// 取款
    /// </summary>
    public class WithdrawRequest
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
        /// <summary>
        /// 存款金額
        /// </summary>
        [Required]
        public decimal Money { get; set; }

        /// <summary>
        /// 存取款,存款:1;取款:0
        /// </summary>
        [Required]
        public int PaymentType { get; set; }
    }
}

