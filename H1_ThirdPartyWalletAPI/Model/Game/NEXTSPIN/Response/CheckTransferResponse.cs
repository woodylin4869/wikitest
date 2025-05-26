using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response
{
    public class CheckTransferResponse : BaseResponse
    {
        /// <summary>
        /// 状态，收到的响应代码
        /// 0 = 转账失败
        /// 1 = 转账成功
        /// </summary>
        [Required]
        public int status { get; set; }

        /// <summary>
        /// 货币的 ISO 代码
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string currency { get; set; }

        /// <summary>
        /// 游戏玩家 ID
        /// </summary>
        [Required]
        [MaxLength(70)]
        public string acctId { get; set; }

        /// <summary>
        /// 转账前的数量
        /// </summary>
        [Required]
        public decimal valueBefore { get; set; }

        /// <summary>
        /// 转账的数量
        /// </summary>
        [Required]
        public decimal valueChange { get; set; }

        /// <summary>
        /// 转账后的数量
        /// </summary>
        [Required]
        public decimal valueAfter { get; set; }
    }
}
