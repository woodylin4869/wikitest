using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.WS168.Response
{
    public class SearchingOrdersStatusResponse
    {

        public string code { get; set; }
        public string message { get; set; }
        public int current_page { get; set; }
        public int total_page { get; set; }
        public int total_count { get; set; }
        public List<Datum> data { get; set; }

        public class Datum
        {   /// <summary>
            /// Bet Number 投注單號 (unique)
            /// </summary>
            public string slug { get; set; }
            /// <summary>
            /// Arena Number 賽事代號
            /// </summary>
            public string arena_fight_no { get; set; }
            public int round_id { get; set; }
            public int fight_no { get; set; }
            /// <summary>
            /// Bet Side 下注哪一邊
            /// </summary>
            public string side { get; set; }
            /// <summary>
            /// User Account 會員帳號名稱
            /// </summary>
            public string account { get; set; }
            /// <summary>
            /// 注單狀態 init 注單初始 beted 注單成立 settled 派彩 cancel 取消 fail 失敗
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// 賠率
            /// </summary>
            public decimal odd { get; set; }
            /// <summary>
            /// 下注金額
            /// </summary>
            public string bet_amount { get; set; }
            /// <summary>
            /// 淨收入(輸贏)
            /// </summary>
            public string net_income { get; set; }
            /// <summary>
            /// 返回金額
            /// </summary>
            public string bet_return { get; set; }
            /// <summary>
            /// 有效投注
            /// </summary>
            public string valid_amount { get; set; }
            public string result { get; set; }
            /// <summary>
            /// 是否已結算
            /// </summary>
            public bool is_settled { get; set; }
            /// <summary>
            /// 投注時間
            /// </summary>
            public DateTime bet_at { get; set; }
            /// <summary>
            /// 結算時間
            /// </summary>
            public DateTime? settled_at { get; set; }
            public string arena_no { get; set; }

            public string pre_bet_amount { get; set; }
            public string pre_net_income { get; set; }
            public string pre_valid_amount { get; set; }

            public Guid summary_id { get; set; }

            public string club_id { get; set; }
            public string franchiser_id { get; set; }

            public DateTime report_time { get; set; }
            public DateTime partition_time { get; set; }
        }
    }
}
