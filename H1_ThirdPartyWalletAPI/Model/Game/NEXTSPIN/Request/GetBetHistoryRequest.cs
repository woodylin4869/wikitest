using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Request
{
    public class GetBetHistoryRequest : BaseRequest
    {
        /// <summary>
        /// 开始时间，格式为 yyyyMMddTHHmmss
        /// </summary>
        [Required]
        [MaxLength(15)]
        public string beginDate { get; set; }

        /// <summary>
        /// 截止时间, 格式为 yyyyMMddTHHmmss
        /// </summary>
        [Required]
        [MaxLength(15)]
        public string endDate { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        [Required]
        public int pageIndex { get; set; }
    }
}
