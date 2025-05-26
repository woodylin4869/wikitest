using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request
{
    /// <summary>
    /// 注單查詢
    /// </summary>
    public class BetRecordByDateRequest
    {
        /// <summary>
        /// 站台代碼（20位）
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PartnerKey { get; set; }
        /// <summary>
        /// 當前拿到的最大ID值
        /// </summary>
        public long Version { get; set; }
        /// <summary>
        /// RefNo
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string RefNo { get; set; }
        /// <summary>
        /// 日期規格：1.1.TransDate / 2.StateUpdateTs
        /// </summary>
        public int TimeType { get; set; }
        /// <summary>
        /// StarDate
        /// </summary>
        [Required]
        [MaxLength(20)]
        public DateTime StartDate { get; set; }
        /// <summary>
        /// EndDate
        /// </summary>
        [Required]
        [MaxLength(20)]
        public DateTime EndDate { get; set; }
    }
}
