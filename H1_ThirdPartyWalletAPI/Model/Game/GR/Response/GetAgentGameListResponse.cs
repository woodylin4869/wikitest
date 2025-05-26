namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// 0021 – 平台取得代理遊戲列表 get_agent_game_list
    /// </summary>
    public class GetAgentGameListResponse : GRResponseBase
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
            /// 後綴碼
            /// </summary>
            public string site_code { get; set; }

            /// <summary>
            /// 遊戲陣列
            /// </summary>
            public Game_List[] game_list { get; set; }

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
        /// 遊戲陣列
        /// </summary>
        public class Game_List
        {
            /// <summary>
            /// 遊戲代碼
            /// </summary>
            public int game_type { get; set; }

            /// <summary>
            /// 遊戲名稱
            /// </summary>
            public string game_name { get; set; }

            /// <summary>
            /// 遊戲種類
            /// </summary>
            public int game_module_type { get; set; }

            /// <summary>
            /// 不同尺寸的遊戲圖檔 url
            /// </summary>
            public Icons_Href icons_href { get; set; }

            /// <summary>
            /// 0. 關閉 / 1. 開啟
            /// </summary>
            public int is_open { get; set; }
        }

        /// <summary>
        /// 不同尺寸的遊戲圖檔 url
        /// </summary>
        public class Icons_Href
        {
            public string icon_150 { get; set; }
            public string icon_200 { get; set; }
            public string icon_300 { get; set; }
        }
    }
}
