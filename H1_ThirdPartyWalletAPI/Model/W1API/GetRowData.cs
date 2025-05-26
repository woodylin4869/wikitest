using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetRowDataReq
    {
        /// <summary>
        /// Game record_id
        /// </summary>
        [StringLength(36)]
        [DefaultValue("179350009")]
        [Required]
        public string record_id { get; set; }
        /// <summary>
        /// summary_id
        /// </summary>
        public Guid summary_id { get; set; }
        /// <summary>
        /// 遊戲平台名
        /// 1. WM
        /// </summary>
        [Required]
        [StringLength(10)]
        [DefaultValue("WM")]
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
        [DefaultValue("2023-04-24 14:40:00")]
        public DateTime ReportTime { get; set; }

    }
    public class GetRowDataRespone : ResCodeBase
    {
        /// <summary>
        /// 遊戲逐筆注單資料
        /// </summary>

        public RCGRowData Data { get; set; }

    }

    public class RCGRowData
    {
        private List<string> betResult = new List<string>();

        public List<object> dataList { get; set; }


        /// <summary>
        /// 下注內容
        /// </summary>
        public List<string> BetResult { get => betResult; set => betResult = value; }

        /// <summary>
        ///  下注金額
        /// </summary>
        public decimal betAmount { get; set; }
        /// <summary>
        /// 注單淨輸贏金額
        /// </summary>
        public decimal netWin { get; set; }

        /// <summary>
        /// 遊戲代碼(gameCode)
        /// </summary>
        public string GameId { get; set; }
    }


}
