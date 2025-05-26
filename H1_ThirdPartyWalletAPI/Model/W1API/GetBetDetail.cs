using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Collections.Generic;
using System;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetBetDetailReq
    {
        /// <summary>
        /// Game record_id
        /// </summary>
        [StringLength(36)]
        [DefaultValue("114385778532")]
        [Required]
        public string record_id { get; set; }
        /// <summary>
        /// summary_id
        /// </summary>
        public Guid summary_id { get; set; }
        /// <summary>
        /// 遊戲平台名
        /// 1. SABA
        /// </summary>
        [Required]
        [StringLength(10)]
        [DefaultValue("SABA")]
        public string Platform { get; set; }
        /// <summary>
        /// 語言
        /// </summary>
        [StringLength(10)]
        [DefaultValue("en-US")]
        public string lang { get; set; }
        /// <summary>
        /// 注單時間
        /// </summary>
        [DefaultValue("2022-03-10 10:25:00")]
        public DateTime ReportTime { get; set; }

    }
    public class GetBetDetail : ResCodeBase
    {
        /// <summary>
        /// 遊戲逐筆注單資料
        /// </summary>
        //public dynamic Data { get; set; }
        //public object Data { get; set; }
        public string Data { get; set; }
    }
}
