namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0007-2 - 平台取得 Slot 下注遊戲後詳細資訊的結果 get_slot_game_round_details
    /// </summary>
    public class GetSlotGameRoundDetailsResponse : GRResponseBase
    {
        /// <summary>
        /// data object
        /// </summary>
        public DataInfo data { get; set; }

        /// <summary>
        /// 參數 data 裡的欄位資料
        /// </summary>
        public class DataInfo
        {
            /// <summary>
            /// 遊戲詳細資料陣列
            /// </summary>
            public Game_Round_Details[] game_round_details { get; set; }

            /// <summary>
            /// 目前所在分頁
            /// </summary>
            public int page_index { get; set; }

            /// <summary>
            /// 每頁筆數
            /// </summary>
            public int page_size { get; set; }

            /// <summary>
            /// 全部筆數
            /// </summary>
            public int total_pages { get; set; }

            /// <summary>
            /// 全部頁數
            /// </summary>
            public int total_elements { get; set; }
        }

        /// <summary>
        /// 遊戲詳細資料陣列
        /// </summary>
        public class Game_Round_Details
        {
            /// <summary>
            /// 局號（字串版），在遊戲及代理後台是顯示十六進制，如下備註，建議使用字串版當作局號，才不會有溢位問題
            /// </summary>
            public string id_str { get; set; }

            /// <summary>
            /// 局號，在遊戲及代理後台是顯示十六進制，如下備註
            /// </summary>
            public long id { get; set; }

            /// <summary>
            /// !!!廠商文件沒有這欄位!!!
            /// </summary>
            public string game_round_hex { get; set; }

            /// <summary>
            /// 遊戲代碼
            /// </summary>
            public int game_type { get; set; }

            /// <summary>
            /// 遊戲資料
            /// </summary>
            public string game_data { get; set; }

            /// <summary>
            /// 結算時間
            /// </summary>
            public string update_time { get; set; }

            /// <summary>
            /// 創建時間
            /// </summary>
            public string create_time { get; set; }
        }
    }
}
