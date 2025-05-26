using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.DataModel;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetGameReportReq
    {
        /// <summary>
        /// 遊戲平台名
        /// 1. MG
        /// </summary>
        [StringLength(10)]
        [DefaultValue("MG")]
        public string Platform { get; set; }
        /// <summary>
        /// 遊戲報表類型
        /// 0.遊戲商提供 1.轉帳中心產出
        /// </summary>
        [StringLength(1)]
        [DefaultValue("0")]
        public string ReportType { get; set; }
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
    }
    public class GetGameReportSummaryReq
    {
        /// <summary>
        /// 遊戲平台名
        /// 1. MG
        /// </summary>
        [StringLength(10)]
        [DefaultValue("MG")]
        public string Platform { get; set; }
        /// <summary>
        /// 遊戲報表類型
        /// 0.遊戲商提供 1.轉帳中心產出
        /// </summary>
        [StringLength(1)]
        [DefaultValue("0")]
        public string ReportType { get; set; }
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
    public class GetGameReport : ResCodeBase
    {
        /// <summary>
        /// 遊戲清單資料
        /// </summary>
        public List<GameReport> Data { get; set; }
    }
    public class GetGameReportSummary : ResCodeBase
    {
        /// <summary>
        /// 遊戲報表筆數
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 遊戲注單總筆數
        /// </summary>
        public int TotalBetCount { get; set; }
        /// <summary>
        /// 遊戲注單總投注
        /// </summary>
        public decimal TotalBet { get; set; }
        /// <summary>
        /// 遊戲注單總贏分(包含投注)
        /// </summary>
        public decimal TotalWin { get; set; }
        /// <summary>
        /// 遊戲注單總淨輸贏(不包含投注)
        /// </summary>
        public decimal TotalNetWin { get; set; }
    }
    public class PutGameReportReq
    {
        /// <summary>
        /// 遊戲報表時間
        /// </summary>
        public DateTime? ReportDateTime { get; set; }
        /// <summary>
        /// 遊戲注單總筆數
        /// </summary>
        public long? TotalCount { get; set; }
        /// <summary>
        /// 遊戲注單總投注
        /// </summary>
        public decimal? TotalBet { get; set; }
        /// <summary>
        /// 遊戲注單總贏分
        /// </summary>
        public decimal? TotalWin { get; set; }
        /// <summary>
        /// 遊戲注單總淨輸贏
        /// </summary>
        public decimal? TotalNetwin { get; set; }
    }
    public class PostGameReportReq
    {
        /// <summary>
        /// 遊戲平台名
        /// </summary>
        [Required]
        public string Platform { get; set; }
        /// <summary>
        /// 遊戲報表時間
        /// </summary>
        [Required]
        public DateTime ReportDateTime { get; set; }
        /// <summary>
        /// 遊戲注單總筆數
        /// </summary>
        [Required]
        public long TotalCount { get; set; }
        /// <summary>
        /// 遊戲注單總投注
        /// </summary>
        [Required]
        public decimal TotalBet { get; set; }
        /// <summary>
        /// 遊戲注單總贏分
        /// </summary>
        [Required]
        public decimal TotalWin { get; set; }
        /// <summary>
        /// 遊戲注單總淨輸贏
        /// </summary>
        [Required]
        public decimal TotalNetwin { get; set; }
        /// <summary>
        /// 遊戲報表類型
        /// 0.遊戲商提供 1.轉帳中心產出
        /// </summary>
        [Required]
        public int ReportType { get; set; }
    }
}
