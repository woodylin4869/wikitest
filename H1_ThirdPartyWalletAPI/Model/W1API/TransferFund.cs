using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using H1_ThirdPartyWalletAPI.Model.DataModel;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class TransferFundReq
    {
        /// <summary>
        /// 唯一交易id
        /// </summary>
        [Required]
        public Guid id { get; set; }
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
        /// <summary>
        /// true:轉出所有餘額　false:轉出amount金額
        /// 僅Action = out 有用
        /// </summary>
        [Required]
        [DefaultValue(false)]
        public bool CashOutAll { get; set; }
    }
    public class TransferFund : ResCodeBase
    {
        /// <summary>
        /// 交易紀錄
        /// </summary>
        public WalletTransferRecord Data { get; set; }
    }
}
