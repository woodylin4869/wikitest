namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Response
{
    public class BetlogResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public List<Page_Info> page_info { get; set; }
        /// <summary>
        /// 此次拿取的頁數
        /// </summary>
        public int current_page { get; set; }
        /// <summary>
        /// 此次拿取注單啟始處 (第n筆)
        /// </summary>
        public int from { get; set; }
        /// <summary>
        /// 此次拿取注單結束處 (第n筆)
        /// </summary>
        public int to { get; set; }
        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int per_page { get; set; }
        /// <summary>
        /// 最後一頁頁數
        /// </summary>
        public int last_page { get; set; }
        /// <summary>
        /// 搜尋時間區間內的總注單數
        /// </summary>
        public int total { get; set; }
        public class Page_Info
        {
            /// <summary>
            /// 注單單號(唯一值)
            /// </summary>
            public string bet_id { get; set; }
            /// <summary>
            /// 局號 (主要棋牌或真人類遊戲注單使用, 預設可不回傳)
            /// </summary>
            public string round { get; set; }
            /// <summary>
            /// 電子:遊戲代碼
            /// 真人:桌別
            /// </summary>
            public string gamecode { get; set; }
            /// <summary>
            /// 玩家遊戲帳號
            /// </summary>
            public string account { get; set; }
            /// <summary>
            /// 幣別 (使用ISO 4217標準)
            /// </summary>
            public string currency { get; set; }
            /// <summary>
            /// 總投注金額
            /// </summary>
            public decimal bet_amount { get; set; }
            /// <summary>
            /// 有效投注金額
            /// </summary>
            public decimal bet_valid_amount { get; set; }
            /// <summary>
            /// 派彩金額(純贏分)
            /// </summary>
            public decimal pay_off_amount { get; set; }
            /// <summary>
            /// 彩金獲得金額
            /// </summary>
            public decimal jp_win { get; set; }
            /// <summary>
            /// 是否包含免費遊戲 (0:不包含, 1:包含, 預設為null:未提供)
            /// </summary>
            public int freegame { get; set; }
            /// <summary>
            /// 投注時間 (GMT+8)
            /// </summary>
            public DateTime bet_time { get; set; }
            /// <summary>
            /// 派彩時間 (GMT+8)
            /// </summary>
            public DateTime pay_off_time { get; set; }
            /// <summary>
            /// 注單狀態 (0:未結算, 1:已結算,2:改單, 3:撤單 )
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// 分區時間
            /// </summary>
            public DateTime partition_time { get; set; }
            /// <summary>
            /// 報表時間
            /// </summary>
            public DateTime report_time { get; set; }
        }
    }
}
