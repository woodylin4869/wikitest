using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Request
{
    public class SiteReportRequest
    {
        /// <summary>
        /// 要查询的下级商户ID
        /// </summary>
        public int? SiteID { get; set; }

        /// <summary>
        /// 时间范围开始时间（东八区） / 格式：yyyy-MM-dd HH:mm:ss
        /// </summary>
        [Required]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// 时间范围结束时间（东八区） / 格式：yyyy-MM-dd HH:mm:ss
        /// </summary>
        [Required]
        public DateTime EndAt { get; set; }

        /// <summary>
        /// 查询页码（默认1）
        /// </summary>
        public int? PageIndex { get; set; }

        /// <summary>
        /// 每页记录数量（默认：20，最大值：1024）
        /// </summary>
        public int? PageSize { get; set; }

    }
}
