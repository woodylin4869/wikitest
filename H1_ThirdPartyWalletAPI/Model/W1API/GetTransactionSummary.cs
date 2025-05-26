using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Attributes;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Utility;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetTransactionSummary_SummaryReq
    {
        /// <summary>
        /// H1 club_id
        /// </summary>
        [StringLength(20)]
        public string Club_id { get; set; }
        /// <summary>
        /// 經銷商
        /// </summary>
        [StringLength(20)]
        public string Franchiser { get; set; }
        /// <summary>
        /// 交易狀態 success/fail/pending
        /// </summary>
        [StringLength(10)]
        public string status { get; set; }
        /// <summary>
        /// 交易類型 IN/OUT/RCG/SABA/JDB
        /// </summary>
        [StringLength(10)]
        public string type { get; set; }
        /// <summary>
        /// 查詢開始時間
        /// </summary>
        [Required]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 查詢結束時間
        /// </summary>
        [Required]
        public DateTime EndTime { get; set; }
    }
    public class GetTransactionSummary_SummaryRes : ResCodeBase
    {
        /// <summary>
        /// 交易紀錄彙總清單資料
        /// </summary>
        public int Count { get; set; }
    }
    public class GetTransactionSummaryReq
    {
        /// <summary>
        /// H1 club_id
        /// </summary>
        [StringLength(20)]
        public string Club_id { get; set; }
        /// <summary>
        /// H1 Franchiser_id
        /// </summary>
        [StringLength(20)]
        public string Franchiser_id { get; set; }
        /// <summary>
        /// 交易狀態 success/fail/pending
        /// </summary>
        [StringLength(10)]
        public string status { get; set; }
        /// <summary>
        /// 交易類型 IN/OUT/RCG/SABA/JDB
        /// </summary>
        [StringLength(10)]
        public string type { get; set; }
        /// <summary>
        /// 查詢開始時間
        /// </summary>
        [Required]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 查詢結束時間
        /// </summary>
        [Required]
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 頁數
        /// </summary>
        [DefaultValue(0)]
        public int? Page { get; set; }
        /// <summary>
        /// 每頁筆數
        /// </summary>
        [DefaultValue(10)]
        public int? Count { get; set; }
        public GetTransactionSummaryReq()
        {
            StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            EndTime = DateTime.Now.AddMinutes(-5);
        }
    }
    public class GetTransactionSummary : ResCodeBase
    {
        /// <summary>
        /// 轉帳記錄資料
        /// </summary>
        public List<WalletTransferRecord> Data { get; set; }

    }
    public class PutTransactionSummaryReq
    {
        /// <summary>
        /// 來源錢包
        /// </summary>
        [StringLength(10)]
        public string source { get; set; }
        /// <summary>
        /// 目標錢包
        /// </summary>
        [StringLength(10)]
        public string target { get; set; }
        /// <summary>
        /// 交易狀態 success/fail/pending
        /// </summary>
        [StringLength(10)]
        public string status { get; set; }
        /// <summary>
        /// 交易類型 IN/OUT/RCG/SABA/JDB
        /// </summary>
        [StringLength(10)]
        public string type { get; set; }
        /// <summary>
        /// 交易金額
        /// </summary>
        [Range(0, 100000000)]
        [DefaultValue(0)]
        public decimal? amount { get; set; }
        /// <summary>
        /// 交易前金額
        /// </summary>
        [Range(0, 100000000)]
        [DefaultValue(0)]
        public decimal? before_balance { get; set; }
        /// <summary>
        /// 交易後金額
        /// </summary>
        [Range(0, 100000000)]
        [DefaultValue(0)]
        public decimal? after_balance { get; set; }
    }

    public class GetTransactionSummaryByPageReq: SortReq
    {
        /// <summary>
        /// H1 club_id
        /// </summary>
        [StringLength(20)]
        public string Club_id { get; set; }
        /// <summary>
        /// H1 Franchiser_id
        /// </summary>
        [StringLength(20)]
        public string Franchiser_id { get; set; }
        /// <summary>
        /// 交易狀態 success/fail/pending
        /// </summary>
        [StringLength(10)]
        public string status { get; set; }
        /// <summary>
        /// 交易類型 IN/OUT/RCG/SABA/JDB
        /// </summary>
        [StringLength(10)]
        public string type { get; set; }
        /// <summary>
        /// 查詢開始時間
        /// </summary>
        [Required]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 查詢結束時間
        /// </summary>
        [Required]
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 頁數
        /// </summary>
        [DefaultValue(0)]
        [Required]
        public int? Page { get; set; }
        /// <summary>
        /// 每頁筆數
        /// </summary>
        [DefaultValue(10)]
        [Required]
        public int? Count { get; set; }
        public GetTransactionSummaryByPageReq()
        {
            StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            EndTime = DateTime.Now.AddMinutes(-5);
        }

        /// <summary>
        /// 排序欄位
        /// "club_id", "success_datetime", "id", "source", "target", "status", "amount", "before_balance", "after_balance"
        /// </summary>
        [InCondition(isCaseSensitivity: false, "club_id", "success_datetime", "id", "source", "target", "status", "amount", "before_balance", "after_balance",ErrorMessage = "SortColumnName is invalid")]
        public override string SortColumnName { get; set; }
        
        /// <summary>
        /// 排序方式
        /// </summary>
        public override SortType? SortType { get; set; }
    }
    public class GetTransactionSummaryByPageResp : ResCodeBase
    {
        /// <summary>
        /// 轉帳記錄資料
        /// </summary>
        public List<WalletTransferRecord> Data { get; set; }

        /// <summary>
        /// 總資料筆數
        /// </summary>
        public int TotalCount { get; set; }
    }


    public class GetElectronicDepositRecord
    {
        /// <summary>
        /// club_id
        /// </summary>
        public string club_id { get; set; }
    }
    public class GetElectronicDepositRecordResponse : ResCodeBase
    {
        /// <summary>
        /// 遊戲逐筆交易資料
        /// </summary>
        public List<WalletTransferRecord> Data { get; set; }
    }
}
