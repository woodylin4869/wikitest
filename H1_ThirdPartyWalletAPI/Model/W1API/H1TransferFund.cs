using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using H1_ThirdPartyWalletAPI.Model.DataModel;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class H1TransferFundReq
    {
        /// <summary>
        /// H1 Session_id
        /// </summary>
        [Required]     
        public Guid Session_id { get; set; }
        /// <summary>
        /// H1 club_id
        /// </summary>
        [Required]
        [StringLength(20)]
        [DefaultValue("10003")]
        public string Club_id { get; set; }
        /// <summary>
        /// 轉帳方式 : in / out
        /// </summary>
        [Required]
        [DefaultValue("in")]
        public string Action { get; set; }
        /// <summary>
        /// 轉帳額度
        /// </summary>
        [Required]
        [Range(0, 100000000)]
        [DefaultValue(10)]
        public decimal Amount { get; set; }
    }
}
