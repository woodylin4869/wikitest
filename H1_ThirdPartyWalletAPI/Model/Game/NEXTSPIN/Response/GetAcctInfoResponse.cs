using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response
{
    public class GetAcctInfoResponse : BaseResponse
    {
        /// <summary>
        /// 总共的记录数
        /// </summary>
        [Required]
        public int resultCount { get; set; }

        /// <summary>
        /// 总共的页数
        /// </summary>
        [Required]
        public int pageCount { get; set; }

        /// <summary>
        /// AcctInfo 类
        /// </summary>
        [Required]
        public AcctInfo[] list { get; set; }

        public class AcctInfo
        {
            /// <summary>
            /// 用户标识 ID
            /// </summary>
            [Required]
            [MaxLength(50)]
            public string acctId { get; set; }

            /// <summary>
            /// 用户名称
            /// </summary>
            [MaxLength(15)]
            public string userName { get; set; }

            /// <summary>
            /// 货币的 ISO 代码
            /// </summary>
            [Required]
            [MaxLength(3)]
            public string currency { get; set; }

            /// <summary>
            /// 用户当前余额
            /// </summary>
            [Required]
            public decimal balance { get; set; }
        }
    }
}
