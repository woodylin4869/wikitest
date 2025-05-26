using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;

/// <summary>
/// 查詢注單
/// </summary>
public class BetLogResponse
{
    /// <summary>
    /// 此次拿取的注單資料
    /// </summary>
    public List<BetLog> page_result { get; set; }

    /// <summary>
    /// 此次拿取的頁數
    /// </summary>
    public int current_page { get; set; }

    /// <summary>
    /// 此次拿取注單啟始處
    /// </summary>
    public int? from { get; set; }

    /// <summary>
    /// 此次拿取注單結束處
    /// </summary>
    public int? to { get; set; }

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

    public class BetLog
    {
        /// <summary>
        /// 遊戲廠商
        /// </summary>
        [MaxLength(10)]
        public string hall { get; set; }

        /// <summary>
        /// 注單單號
        /// 同遊戲商內注單唯一值
        /// </summary>
        [MaxLength(50)]
        public string rowid { get; set; }

        /// <summary>
        /// 遊戲局號
        /// </summary>
        [MaxLength(255)]
        public string round { get; set; }

        /// <summary>
        /// 遊戲分類
        /// 
        /// 電子 => electronic
        /// 真人 => live
        /// 彩票 => lottery
        /// 運動 => sport
        /// 捕魚 => fishing
        /// 棋牌 => board
        /// 電竸 => e-sports
        /// </summary>
        [MaxLength(20)]
        public string category { get; set; }

        /// <summary>
        /// 遊戲代碼
        /// </summary>
        [MaxLength(50)]
        public string gameid { get; set; }

        /// <summary>
        /// 遊戲名稱
        /// </summary>
        [MaxLength(50)]
        public string game_name { get; set; }

        /// <summary>
        /// 玩家帳號
        /// </summary>
        [MaxLength(20)]
        public string casino_account { get; set; }

        /// <summary>
        /// 有效投注金額
        /// 取消單(status=3)該欄位為0
        /// </summary>
        [MaxLength(15)]
        public string betvalid { get; set; }

        /// <summary>
        /// 投注金額
        /// </summary>
        [MaxLength(15)]
        public string betamount { get; set; }

        /// <summary>
        /// 派彩金額
        /// 玩家下注15 返還0 -> betresult=-15
        /// 玩家下注15 返還15 -> betresult=0
        /// 玩家下注15 返還30 -> betresult=15
        /// 
        /// 未派彩注單派彩金額:
        /// 下注15 真人系列派彩金額為-15
        /// 下注15 彩票系列派彩金額為0
        /// 
        /// 取消單(status=3)該欄位為0
        /// </summary>
        [MaxLength(15)]
        public string betresult { get; set; }

        /// <summary>
        /// 彩池貢獻金
        /// </summary>
        [MaxLength(15)]
        public string pca_contribute { get; set; }

        /// <summary>
        /// 彩池中獎金額
        /// </summary>
        [MaxLength(15)]
        public string pca_win { get; set; }

        /// <summary>
        /// 抽水金額
        /// 抽水：玩家贏錢就需繳費，抽水值為正數，只有RM系列會有抽水
        /// 例如:
        /// 下注10，贏20，抽水2，派彩即為+8
        /// </summary>
        [MaxLength(15)]
        public string revenue { get; set; }

        /// <summary>
        /// 投注時間
        /// </summary>
        [MaxLength(25)]
        public DateTime bettime { get; set; }

        /// <summary>
        /// 派彩時間
        /// Optional
        /// </summary>
        [MaxLength(25)]
        public DateTime payout_time { get; set; }

        /// <summary>
        /// 注單爬回時間
        /// </summary>
        [MaxLength(25)]
        public DateTime reporttime { get; set; }

        /// <summary>
        /// 是否追號
        /// true = 是, false = 否
        /// </summary>
        public bool trace { get; set; }

        /// <summary>
        /// 是否包含免費遊戲
        /// 0:不包含, 1:包含, 預設為null:未提供
        /// </summary>
        public int? freegame { get; set; }

        /// <summary>
        /// 下注類型
        /// 預設為null:未提供
        /// </summary>
        public string bettype { get; set; }

        /// <summary>
        /// 遊戲結果
        /// 預設為null:未提供
        /// </summary>
        public string gameresult { get; set; }

        /// <summary>
        /// 注單狀態
        /// 0:未派彩, 1:已派彩, 2:已派彩但注單修改過, 3:撤單
        /// </summary>
        [MaxLength(1)]
        public string status { get; set; }

        #region Customization Field
        /// <summary>
        /// 彙總帳時間
        /// </summary>
        public DateTime db_report_time { get; set; }


        /// <summary>
        /// 原始下注金額
        /// 
        /// 該欄位為真人系列針對改牌邏輯設計
        /// 電子遊戲該欄位無意義
        /// </summary>
        public string pre_betamount { get; set; }

        /// <summary>
        /// 原始有效下注
        /// 
        /// 該欄位為真人系列針對改牌邏輯設計
        /// 電子遊戲該欄位無意義
        /// </summary>
        public string pre_betvalid { get; set; }

        /// <summary>
        /// 原始淨輸贏
        /// 
        /// 該欄位為真人系列針對改牌邏輯設計
        /// 電子遊戲該欄位無意義
        /// </summary>
        public string pre_betresult { get; set; }

        public bool isLiveRecord => Array.IndexOf(new string[] { "live" }, category) != -1;

        public bool isElectronicRecord => Array.IndexOf(new string[] { "electronic", "fishing" }, category) != -1;
        /// <summary>
        /// 分區時間
        /// </summary>
        [MaxLength(25)]
        public DateTime partition_time { get; set; }
        #endregion
    }
}
