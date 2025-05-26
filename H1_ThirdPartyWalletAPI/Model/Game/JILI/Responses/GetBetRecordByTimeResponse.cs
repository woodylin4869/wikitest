using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses
{
    public class GetBetRecordByTimeResponse
    {


        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public GetBetRecordByTimeData Data { get; set; }

        public class GetBetRecordByTimeData
        {
            public List<Result> Result { get; set; }
            public Pagination Pagination { get; set; }
        }

        public class Pagination
        {
            /// <summary>
            /// 當前頁數
            /// </summary>
            public int CurrentPage { get; set; }
            /// <summary>
            /// 總頁數
            /// </summary>
            public int TotalPages { get; set; }
            /// <summary>
            /// 每頁筆數
            /// </summary>
            public int PageLimit { get; set; }
            /// <summary>
            /// 總筆數
            /// </summary>
            public int TotalNumber { get; set; }
        }

        public class Result
        {
            /// <summary>
            /// 會員唯一識別值
            /// </summary>
            public string Account { get; set; }
            /// <summary>
            /// 在遊戲內注單唯一值
            /// </summary>
            public long WagersId { get; set; }
            /// <summary>
            /// 遊戲的唯一識別值
            /// </summary>
            public int GameId { get; set; }
            /// <summary>
            /// 投注時間
            /// </summary>
            public DateTime WagersTime { get; set; }
            /// <summary>
            /// 投注金額
            /// </summary>
            public decimal BetAmount { get; set; }
            /// <summary>
            /// 派彩時間
            /// </summary>
            public DateTime PayoffTime { get; set; }
            /// <summary>
            /// 派彩金額
            /// </summary>
            public decimal PayoffAmount { get; set; }
            /// <summary>
            /// 1: 贏 2: 輸 3: 平局
            /// </summary>
            public int Status { get; set; }
            /// <summary>
            /// 對帳時間
            /// </summary>
            public DateTime SettlementTime { get; set; }
            /// <summary>
            /// 請參考 附錄 – 遊戲類型
            /// </summary>
            public int GameCategoryId { get; set; }

            public int VersionKey { get; set; }
            /// <summary>
            /// 請參考 附錄 – 注單類型
            /// </summary>
            public int Type { get; set; }
            /// <summary>
            /// 會員所屬站長唯一識別值
            /// </summary>
            public string AgentId { get; set; }
            /// <summary>
            /// 有效投注金額
            /// </summary>
            public decimal  Turnover { get; set; }

            public Guid summary_id { get; set; }


            public DateTime report_time { get; set; }
        }

    }
}
