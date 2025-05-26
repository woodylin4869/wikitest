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
    public class GetBetRecordUnsettleReq
    {
        /// <summary>
        /// 遊戲平台名
        /// 1. SABA
        /// </summary>
        [Required]
        [StringLength(10)]
        [DefaultValue("SABA")]
        public string Platform { get; set; }
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
        /// 查詢開始時間
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 查詢結束時間
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
    public class GetBetRecordUnsettleRes : ResCodeBase
    {
        /// <summary>
        /// 遊戲逐筆注單資料
        /// </summary>
        public List<dynamic> Data { get; set; }
    }
}