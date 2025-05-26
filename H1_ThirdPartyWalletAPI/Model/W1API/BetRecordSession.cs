using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Model.DataModel;
namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetBetRecordSessionReq
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
        /// game_id
        /// </summary>
        [StringLength(10)]
        public string game_id { get; set; }
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
        /// 查詢時間類型 1:ReportTime 2:UpdateTime
        /// </summary>
        [DefaultValue(2)]
        public int SearchType { get; set; }
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

        public GetBetRecordSessionReq()
        {
            StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            EndTime = DateTime.Now.AddMinutes(-5);
        }
    }
    public class GetBetRecordSessionRes : ResCodeBase
    {
        public List<BetRecordSession> Data { get; set; }
    }
    public class GetBetRecordSession_SummaryReq
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
        /// game_id
        /// </summary>
        [StringLength(10)]
        public string game_id { get; set; }
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
        /// 查詢時間類型 1:ReportTime 2:UpdateTime
        /// </summary>
        [DefaultValue(2)]
        public int SearchType { get; set; }

        public GetBetRecordSession_SummaryReq()
        {
            StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            EndTime = DateTime.Now.AddMinutes(-5);
        }
    }
    public class GetBetRecordSession_SummaryRes : ResCodeBase
    {
        /// <summary>
        /// 下注紀錄彙總清單資料
        /// </summary>
        public int Count { get; set; }
    }
    public class PutBetRecordSessionReq
    {
        /// <summary>
        /// 投注金額
        /// </summary>
        [Range(0, 100000000)]
        [DefaultValue(0)]
        public decimal? bet_amount { get; set; }
        /// <summary>
        /// 有效投注
        /// </summary>
        [Range(0, 100000000)]
        [DefaultValue(0)]
        public decimal? turnover { get; set; }
        /// <summary>
        /// 贏分
        /// </summary>
        [Range(0, 100000000)]
        [DefaultValue(0)]
        public decimal? win { get; set; }
        /// <summary>
        /// 淨輸贏
        /// </summary>
        [Range(0, 100000000)]
        [DefaultValue(0)]
        public decimal? netwin { get; set; }
        /// <summary>
        /// 筆數
        /// </summary>
        [Range(0, 100000000)]
        [DefaultValue(0)]
        public int? recordcount { get; set; }
    }
}