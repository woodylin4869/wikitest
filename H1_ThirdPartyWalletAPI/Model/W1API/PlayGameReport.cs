using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetPlayGameReportReq
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        [Required]
        [StringLength(20)]
        [DefaultValue("10003")]
        public string Club_id { get; set; }

        /// <summary>
        /// 報表日期 yyyy-MM-dd
        /// </summary>
        [Required]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "日期格式必須為 yyyy-MM-dd")]
        public string ReportTime { get; set; }
    }


    public class GetPlayGameReport : ResCodeBase
    {
        /// <summary>
        /// 遊戲清單資料
        /// </summary>
        public List<PlayGameReport> Data { get; set; }
    }
    public class PlayGameReport
    {
        /// <summary>
        /// 會員ID
        /// </summary>
        public string Club_id { get; set; }

        /// <summary>
        /// 遊戲館
        /// </summary>
        public string platform { get; set; }

        /// <summary>
        /// 遊戲ID
        /// </summary>
        public string game_id { get; set; }

        /// <summary>
        /// 遊戲注單總筆數
        /// </summary>
        public long TotalBetCount { get; set; }

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

        /// <summary>
        /// 彩金
        /// </summary>
        public decimal JackPot { get; set; }

        /// <summary>
        /// 匯總時間
        /// </summary>
        public string report_time { get; set; }
    }
}
