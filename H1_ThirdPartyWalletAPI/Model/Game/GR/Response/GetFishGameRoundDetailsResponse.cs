namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0007-3 - 平台取得魚機遊戲結算後詳細資訊 get_fish_game_round_details
    /// </summary>
    public class GetFishGameRoundDetailsResponse : GRResponseBase
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
            //public string game_round_hex { get; set; }

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
            //public string update_time { get; set; }

            /// <summary>
            /// 創建時間
            /// </summary>
            public string create_time { get; set; }

            /// <summary>
            /// 廳號
            /// </summary>
            public int room_id { get; set; }

            /// <summary>
            /// 桌號
            /// </summary>
            public int table_id { get; set; }

            /// <summary>
            /// 局號
            /// </summary>
            public string game_round { get; set; }

            /// <summary>
            /// 總下注金額
            /// </summary>
            public decimal total_bet { get; set; }

            /// <summary>
            /// 總贏得金額
            /// </summary>
            public decimal total_win { get; set; }

            /// <summary>
            /// 總子彈數量
            /// </summary>
            public int total_bullet_count { get; set; }
        }
    }
}
