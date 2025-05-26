using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetBetRecordReq
    {
        /// <summary>
        /// H1 summary_id
        /// </summary>
        [StringLength(36)]
        [DefaultValue("60b9e31e-d16b-4f38-8fb2-d325d65ea37b")]
        [Required]
        public string summary_id { get; set; }
        /// <summary>
        /// 遊戲平台名
        /// 1. SABA
        /// </summary>
        [Required]
        [StringLength(10)]
        [DefaultValue("SABA")]
        public string Platform { get; set; }
        /// <summary>
        /// 注單時間
        /// </summary>
        [Required]
        [DefaultValue("2022-03-10 10:25:00")]
        public DateTime ReportTime { get; set; }

        /// <summary>
        /// Club_id
        /// </summary>
        [Required]
        public string ClubId { get; set; }
    }
    public class GetBetRecord : ResCodeBase
    {
        /// <summary>
        /// 遊戲逐筆注單資料
        /// </summary>
        public List<dynamic> Data { get; set; }
    }
}
