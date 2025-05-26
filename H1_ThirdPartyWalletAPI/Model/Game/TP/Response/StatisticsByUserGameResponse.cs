using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;

/// <summary>
/// 查詢投注量統計_依玩家與遊戲
/// </summary>
public class StatisticsByUserGameResponse
{
    /// <summary>
    /// 此次拿取的投注統計資料
    /// </summary>
    public List<Statistics> data { get; set; }

    /// <summary>
    /// 此次拿取的頁數
    /// </summary>
    public int current_page { get; set; }

    /// <summary>
    /// 此次拿取資料啟始處
    /// </summary>
    public int? from { get; set; }

    /// <summary>
    /// 此次拿取資料結束處
    /// </summary>
    public int? to { get; set; }

    /// <summary>
    /// 每頁筆數
    /// </summary>
    public short per_page { get; set; }

    /// <summary>
    /// 最後一頁頁數
    /// </summary>
    public int last_page { get; set; }

    /// <summary>
    /// 搜尋時間區間內資料總筆數
    /// </summary>
    public int total { get; set; }



    /// <summary>
    /// 投注統計資料
    /// </summary>
    public class Statistics
    {
        /// <summary>
        /// 玩家帳號
        /// </summary>
        public string player_account { get; set; }

        /// <summary>
        /// 娛樂城代碼
        /// </summary>
        public string gamehall { get; set; }

        /// <summary>
        /// 遊戲代碼
        /// </summary>
        public string gamecode { get; set; }

        /// <summary>
        /// 遊戲分類
        /// </summary>
        public string category { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// 總投注額
        /// </summary>
        public decimal bet_amount { get; set; }

        /// <summary>
        /// 總有效投注額
        /// </summary>
        public decimal bet_value { get; set; }

        /// <summary>
        /// 總注單數
        /// </summary>
        public int bet_count { get; set; }

        /// <summary>
        /// 總派彩(玩家輸贏)
        /// </summary>
        public decimal bet_result { get; set; }

        /// <summary>
        /// 贏的注單數
        /// </summary>
        public int win_count { get; set; }

        /// <summary>
        /// 贏的注單派彩
        /// </summary>
        public decimal win_point { get; set; }
    }
}
