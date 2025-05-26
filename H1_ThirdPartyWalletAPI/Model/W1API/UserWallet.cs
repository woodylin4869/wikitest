using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using H1_ThirdPartyWalletAPI.Model.DataModel;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetUserWalletReq
    {
        /// <summary>
        /// 頁數
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        [DefaultValue(0)]
        public int Page { get; set; }
        /// <summary>
        /// 每頁筆數
        /// </summary>
        [Required]
        [Range(1, 1000)]
        [DefaultValue(10)]
        public int Count { get; set; }
        /// <summary>
        /// 經銷商
        /// </summary>
        [StringLength(20)]
        public string Franchiser { get; set; }
        /// <summary>
        /// 使用者Ename
        /// </summary>
        [StringLength(20)]
        public string Ename { get; set; }
    }
    public class GetUserWalletRes : ResCodeBase
    {
        /// <summary>
        /// 玩家錢包清單資料
        /// </summary>
        public List<Wallet> Data { get; set; }
    }
    public class GetUserWalletSummaryReq
    {
        /// <summary>
        /// 經銷商
        /// </summary>
        [StringLength(20)]
        public string Franchiser { get; set; }
    }
    public class GetUserWalletSummaryRes : ResCodeBase
    {
        /// <summary>
        /// 玩家錢包清單資料
        /// </summary>
        public int Count { get; set; }
    }
    public class PutUserWalletReq
    {
        /// <summary>
        /// 遊戲名稱-緬文       
        /// </summary>
        [StringLength(20)]
        public string franchiser_id { get; set; }
        /// <summary>
        /// 使用者錢包餘額     
        /// </summary>
        [Range(0, 100000000)]
        [DefaultValue(0)]
        public decimal? credit { get; set; }
        /// <summary>
        /// 使用者錢包鎖定餘額       
        /// </summary>
        [Range(0, 100000000)]
        [DefaultValue(0)]
        public decimal? lock_credit { get; set; }

    }
    public class CreateUserReq
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        [Required]
        [StringLength(20)]
        [DefaultValue("10003")]
        public string Club_id { get; set; }
        /// <summary>
        /// 使用者帳號
        /// </summary>
        [Required]
        [StringLength(20)]
        [DefaultValue("H1Test003")]
        public string Club_Ename { get; set; }
        /// <summary>
        /// 代理商ID
        /// </summary>
        [Required]
        [StringLength(20)]
        [DefaultValue("000001")]
        public string Franchiser_id { get; set; }
        /// <summary>
        /// 幣別
        /// 1. THB 泰銖
        /// 2. UUS 測試幣
        /// </summary>
        [Required]
        [StringLength(10)]
        [DefaultValue("THB")]
        public string Currency { get; set; }
    }
    public class CreateUserRes : ResCodeBase
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        public string Club_id { get; set; }
    }
    public class StopBalanceReq
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        [Required]
        [StringLength(20)]
        [DefaultValue("10003")]
        public string Club_id { get; set; }
        /// <summary>
        /// 停利額度
        /// </summary>
        [Required]
        [DefaultValue(-1)]
        public decimal stop_balance { get; set; }
    }
}
