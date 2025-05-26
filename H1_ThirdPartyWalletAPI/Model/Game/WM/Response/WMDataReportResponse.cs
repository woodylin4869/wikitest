using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.WM.Response
{
    public class WMDataReportResponse
    {

        public int errorCode { get; set; }
        public string errorMessage { get; set; }
        public List<Result> result { get; set; }


        public class Result
        {
            public string user { get; set; }
            public string betId { get; set; }
            public DateTime betTime { get; set; }
            /// <summary>
            /// 下注錢金額
            /// </summary>
            public decimal beforeCash { get; set; }
            /// <summary>
            /// 下注金額
            /// </summary>
            public decimal bet { get; set; }
            /// <summary>
            /// 有效下注
            /// </summary>
            public decimal validbet { get; set; }
            /// <summary>
            /// 退水
            /// </summary>
            public decimal water { get; set; }
            /// <summary>
            /// 下注結果
            /// </summary>
            public string result { get; set; }
            /// <summary>
            /// 下注代碼
            /// </summary>
            public string betCode { get; set; }
            /// <summary>
            /// 下注退水金額
            /// </summary>
            public decimal waterbet { get; set; }
            /// <summary>
            /// 輸贏金額
            /// </summary>
            public decimal winLoss { get; set; }
            /// <summary>
            /// 遊戲類型編號
            /// </summary>
            public int gid { get; set; }
            /// <summary>
            /// 結算時間
            /// </summary>
            public DateTime settime { get; set; }
            /// <summary>
            /// 有無重對
            /// </summary>
            public string reset { get; set; }
            /// <summary>
            /// 下注內容
            /// </summary>
            public string betResult { get; set; }
            /// <summary>
            /// 牌型
            /// </summary>
            public string gameResult { get; set; }
            public Guid summary_id { get; set; }
            public decimal pre_bet { get; set; }
            public decimal pre_validbet { get; set; }
            public decimal pre_winLoss { get; set; }
            public string gname { get; set; }


            /// <summary>
            /// IP
            /// </summary>
            public string ip { get; set; }

            /// <summary>
            /// 场次编号
            /// </summary>
            [JsonProperty("event")]
            public string Event { get; set; }
            /// <summary>
            /// 子场次编号
            /// </summary>
            public string eventChild { get; set; }
            /// <summary>
            /// 场次编号
            /// </summary>
            public string round { get; set; }
            /// <summary>
            /// 子场次编号
            /// </summary>
            public string subround { get; set; }
            /// <summary>
            /// 桌台编号
            /// </summary>
            public string tableId { get; set; }
            /// <summary>
            /// 傭金類型 0:一般, 1:免佣
            /// </summary>
            public string commission { get; set; }
            /// <summary>
            /// 報表時間
            /// </summary>
            public DateTime report_time { get; set; }
            /// <summary>
            /// 分區時間
            /// </summary>
            public DateTime partition_time { get; set; }
        }
    }
}
