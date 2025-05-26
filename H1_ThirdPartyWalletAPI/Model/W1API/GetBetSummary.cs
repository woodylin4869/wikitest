using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Attributes;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetBetSummary_SummaryReq
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

        public GetBetSummary_SummaryReq()
        {
            StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            EndTime = DateTime.Now.AddMinutes(-5);
        }
    }

    public class GetBetSummary_SummaryRes : ResCodeBase
    {
        /// <summary>
        /// 下注紀錄彙總清單資料
        /// </summary>
        public int Count { get; set; }
    }

    public class GetBetSummaryReq
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

        public GetBetSummaryReq()
        {
            StartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            EndTime = DateTime.Now.AddMinutes(-5);
        }
    }

    public class RepairBetSummaryReq
    {
        /// <summary>
        /// game_id
        /// </summary>
        [Required]
        [StringLength(10)]
        public string game_id { get; set; }

        /// <summary>
        /// 查詢時間類型 1:BetTime 2:SettleTime
        /// </summary>
        [DefaultValue(2)]
        public int SearchType { get; set; }

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

    public class GetBetSummary : ResCodeBase
    {
        /// <summary>
        /// 彙總注單資料
        /// </summary>
        public List<BetRecordSummary> Data { get; set; }
    }

    public class PutBetSummaryReq
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

    public class RepairBetSummaryReqTest : RepairBetSummaryReq
    {
        /// <summary>
        /// 明細流水號起始編號
        /// </summary>
        public int InsertSequenceStartNumber { get; set; }

        /// <summary>
        /// 產生資料筆數
        /// </summary>
        public int GenerateRecordCount { get; set; }
    }



    public class RepairLogByPageReq
    {
        /// <summary>
        /// 頁數
        /// </summary>
        [DefaultValue(1)]
        [Required]
        public int? Page { get; set; }
        /// <summary>
        /// 每頁筆數
        /// </summary>
        [DefaultValue(10)]
        [Required]
        public int? Count { get; set; }
    }
    public class RepairLogByPageResp : ResCodeBase
    {
        /// <summary>
        /// 記錄資料
        /// </summary>
        public List<RepairLogModel> Data { get; set; }

        /// <summary>
        /// 總資料筆數
        /// </summary>
        public long TotalCount { get; set; }
    }
}