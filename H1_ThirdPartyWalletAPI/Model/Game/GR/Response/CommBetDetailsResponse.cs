using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    public class CommBetDetailsResponse : GRResponseBase
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
            /// 下注資料陣列
            /// </summary>
            public List<CommBetDetails> bet_details { get; set; }

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
        /// Fish 下注資料陣列
        /// Slot 下注資料陣列
        /// 共用 下注資料陣列 合併以上各自沒有的欄位
        /// </summary>
        public class CommBetDetails : GRBetDetailsResponseBase
        {
            /// <summary>
            /// Slot
            /// 遊戲局號（字串版），在遊戲及代理後台是顯示十六進制，如下備註。（建議使用字串版的當作遊戲局號，才不會因數字的溢位而造成的資料錯誤）
            /// </summary>
            public string? game_round_str { get; set; }

            /// <summary>
            /// Slot
            /// !!!廠商文件沒有這欄位!!!
            /// </summary>
            public string? game_round_hex { get; set; }

            /// <summary>
            /// Fish
            /// 廳號
            /// </summary>
            public decimal? room_id { get; set; }

            /// <summary>
            /// Fish
            /// 桌號
            /// </summary>
            public decimal? table_id { get; set; }

            /// <summary>
            /// Fish
            /// 子彈數量
            /// </summary>
            public int? bullet_count { get; set; }
        }

        /// <summary>
        ///
        /// GR 下注資料陣列 共用回傳欄位
        ///
        /// 0006-2 - 平台取得 Slot 使用者下注歷史資料 get_slot_all_bet_details
        /// 0006-3 - 平台取得魚機使用者下注歷史資料 get_fish_all_bet_details
        /// </summary>
        public class GRBetDetailsResponseBase
        {
            /// <summary>
            /// 注單號流水號（字串版），建議使用字串版當作注單流水號，才不會有溢位問題
            /// </summary>
            public string id_str { get; set; }

            /// <summary>
            /// 注單號流水號
            /// </summary>
            public long id { get; set; }

            /// <summary>
            /// 系統的唯一碼( {game_module_type}_{注單號流水號} )
            /// </summary>
            public string sid { get; set; }

            /// <summary>
            /// 使用者帳號
            /// </summary>
            public string account { get; set; }

            /// <summary>
            /// 遊戲代碼
            /// </summary>
            public int game_type { get; set; }

            /// <summary>
            /// 遊戲模組代碼對應表
            /// 1 百人遊戲(ex: 百家樂, 射龍門等)
            /// 2 對戰遊戲(ex: 對戰炸金花, 搶莊牛牛等)
            /// 3 Slot 遊戲(ex: 火鳳凰, 埃及豔后等)
            /// 4 魚機遊戲(ex: 龍王捕魚)
            /// </summary>
            public int game_module_type { get; set; }

            /// <summary>
            /// 遊戲局號,，在遊戲及代理後台是顯示十六進制, 如下備註。
            /// </summary>
            public long game_round { get; set; }

            /// <summary>
            /// 遊戲局號（字串版），在遊戲及代理後台是顯示十六進制，如下備註。（建議使用字串版的當作遊戲局號，才不會因數字的溢位而造成的資料錯誤）
            /// </summary>
            //public string game_round_str { get; set; }

            /// <summary>
            /// !!!廠商文件沒有這欄位!!!
            /// </summary>
            //public string game_round_hex { get; set; }

            /// <summary>
            /// 下注金額
            /// </summary>
            public decimal bet { get; set; }

            /// <summary>
            /// 賽局結果
            /// </summary>
            public string game_result { get; set; }

            /// <summary>
            /// 有效投注
            /// </summary>
            public decimal valid_bet { get; set; }

            /// <summary>
            /// 贏得金額
            /// </summary>
            public decimal win { get; set; }

            /// <summary>
            /// 創建時間
            /// </summary>
            public DateTime create_time { get; set; }

            /// <summary>
            /// 單號
            /// </summary>
            public string order_id { get; set; }

            /// <summary>
            /// 玩家登入的設備
            /// </summary>
            public string device { get; set; }

            /// <summary>
            /// 玩家登入 IP
            /// </summary>
            public string client_ip { get; set; }

            /// <summary>
            /// 點數類型(real 真實貨幣)
            /// </summary>
            public string c_type { get; set; }

            /// <summary>
            /// 獲利
            /// </summary>
            public decimal profit { get; set; }

            /// <summary>
            /// 彙總帳時間
            /// </summary>
            public DateTime report_time { get; set; }
            public DateTime partition_time { get; set; }
        }
    }
}