using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response
{
    public class DepositResponse : BaseResponse
    {
        /// <summary>
        /// 交易流水号
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string transactionId { get; set; }

        /// <summary>
        /// 玩家最新余额
        /// </summary>
        [Required]
        public decimal afterBalance { get; set; }
    }
}
