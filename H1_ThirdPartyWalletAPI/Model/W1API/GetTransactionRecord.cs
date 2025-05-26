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
    public class GetTransactionRecordReq
    {
        /// <summary>
        /// H1 summary_id
        /// </summary>
        [StringLength(36)]
        [DefaultValue("3618c74d-0984-466b-8e5b-c82425eb2cd4")]
        [Required]
        public string summary_id { get; set; }
        /// <summary>
        /// 遊戲平台名
        /// 1.SABA 2.RCG
        /// </summary>
        [Required]
        [StringLength(10)]
        [DefaultValue("RCG")]
        public string Platform { get; set; }
    }
    public class GetTransactionRecord : ResCodeBase
    {
        /// <summary>
        /// 遊戲逐筆交易資料
        /// </summary>
        public List<dynamic> Data { get; set; }
    }
}
