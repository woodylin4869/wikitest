using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Response
{
    /// <summary>
    /// 取得會員下注內容
    /// </summary>
    public class GetBetRecordByTimeResponse : BaseResponse
    {
        /// <summary>
        /// Data object
        /// </summary>
        public DataInfo Data { get; set; }

        /// <summary>
        /// 參數 data 裡的欄位資料
        /// </summary>
        public class DataInfo
        {
            /// <summary>
            /// 分頁與總計 陣列
            /// </summary>
            public Pagination Pagination { get; set; }

            /// <summary>
            /// 下注資料 陣列
            /// </summary>
            public List<Result> Result { get; set; }
        }

        /// <summary>
        /// 分頁與總計 陣列
        /// </summary>
        public class Pagination
        {
            /// <summary>
            /// 當前頁數
            /// </summary>
            public int CurrentPage { get; set; }

            /// <summary>
            /// 每頁筆數
            /// </summary>
            public int PageLimit { get; set; }

            /// <summary>
            /// 總頁數
            /// </summary>
            public int TotalPages { get; set; }

            /// <summary>
            /// 資料數目
            /// </summary>
            public int TotalNumber { get; set; }
        }

        /// <summary>
        /// 注單內容 陣列
        /// </summary>
        public class Result
        {
            /// <summary>
            /// w1共用 summary_id
            /// </summary>
            public Guid summary_id { get; set; }

            /// <summary>
            /// 會員帳號，限英數字及_線，長度4~30字
            /// </summary>
            public string Account { get; set; }

            /// <summary>
            /// 注單編號(等於 BetFormId)
            /// </summary>
            public string WagersId { get; set; }

            /// <summary>
            /// 遊戲類別
            /// 百家樂	1	已上線
            /// 骰寶	2	已上線
            /// 輪盤	3	已上線
            /// 多桌	4	已上線，僅供會員進入遊戲時使用，注單資料不會有此遊戲類別
            /// 龍虎	5	已上線
            /// 色碟	6	已上線
            /// 極速骰寶	7	開發中
            /// </summary>
            public int GameType { get; set; }

            /// <summary>
            /// 下注金額
            /// </summary>
            public decimal BetAmount { get; set; }

            /// <summary>
            /// 有效金額
            /// </summary>
            public decimal validBetAmount { get; set; }

            /// <summary>
            /// 下注時間(UTC-4) 2020-04-11T14:15:26
            /// </summary>
            public DateTime WagersTime { get; set; }

            /// <summary>
            /// 更新時間(UTC-4)，發生結算/取消/改單時都會更新此時間
            /// </summary>
            public DateTime PayoffTime { get; set; }

            /// <summary>
            /// 下注時間(UTC-4)，同 WagersTime
            /// </summary>
            public DateTime SettlementTime { get; set; }

            /// <summary>
            /// 輸贏金額
            /// </summary>
            public decimal PayoffAmount { get; set; }

            /// <summary>
            /// 獎金，棄用，alwasy 0
            /// </summary>
            //public decimal? Jackpot { get; set; }

            /// <summary>
            /// 退水，尚未支援，alwasy 0
            /// </summary>
            public decimal Commission { get; set; }

            /// <summary>
            /// 注單狀態
            /// 1	中獎	玩家在任一注區有贏錢
            /// 2	未中獎 玩家沒有在任何注區贏錢
            /// 3	和局 輸贏金額 0 且 有效金額 0
            /// 4	進行中 非同步注單不會有進行中的單
            /// 6	取消單	
            /// 7	改單
            /// </summary>
            public int Status { get; set; }

            /// <summary>
            /// 貢獻金，棄用，alwasy 0
            /// </summary>
            //public decimal? Contribution { get; set; }

            /// <summary>
            /// 此幣別需該代理有啟用才能使用
            /// </summary>
            public string Currency { get; set; }

            /// <summary>
            /// 遊戲玩法
            /// 百家樂標準	1
            /// 百家樂西洋	2
            /// 百家樂免水	3
            /// </summary>
            public int GameMethod { get; set; }

            /// <summary>
            /// 棄用, only 2
            /// </summary>
            public int TableType { get; set; }

            /// <summary>
            /// 遊戲局桌檯Id
            /// 實際開桌狀況以遊戲顯示為準
            /// 遊戲類別 Table Id
            /// Baccarat    E, F, G, H, I, J, K, L, O, P，I, J 為特色百家
            /// DragonTiger A
            /// Roulette B
            /// Sicbo V, W
            /// Sedie C, D
            /// </summary>
            public string TableId { get; set; }

            /// <summary>
            /// 輪號(百家和龍虎是 一個牌靴一輪,輪盤和色碟,骰寶,極速骰寶一工作天為一輪，直至下一個工作天，輪號才可更動)
            /// </summary>
            public int Round { get; set; }

            /// <summary>
            /// 局號(依每局結束而更動)
            /// </summary>
            public int Run { get; set; }

            /// <summary>
            /// 遊戲結果
            /// 百家樂時，順序：[閒1,莊1,閒2,莊2,閒補,莊補]，若空的表示無補牌，ex: D2,DK,C4,H7,,
            /// 骰寶時，順序：[骰子點數, 骰子點數, 骰子點數]，ex: 3,5,1
            /// 輪盤時，順序：[球號]，ex: 15
            /// 龍虎時，順序：[龍, 虎]，ex: C4,H7
            /// 色碟時，順序：[紅色顆數]，ex: 2
            /// 極速骰寶時，順序：[骰子點數]，ex: 3
            /// </summary>
            public string? GameResult { get; set; }

            /// <summary>
            /// 注區下注列表
            /// </summary>
            public string? BetType { get; set; }
            //public BetType BetType { get; set; }

            /// <summary>
            /// 單注列表
            /// </summary>
            public string Transactions { get; set; }
            //public Transactions Transactions { get; set; }

            /// <summary>
            /// pre下注金額
            /// </summary>
            public decimal pre_BetAmount { get; set; }

            /// <summary>
            /// pre有效金額
            /// </summary>
            public decimal pre_validBetAmount { get; set; }

            /// <summary>
            /// pre輸贏金額
            /// </summary>
            public decimal pre_PayoffAmount { get; set; }

            /// <summary>
            /// pre注單狀態
            /// 1	中獎	玩家在任一注區有贏錢
            /// 2	未中獎 玩家沒有在任何注區贏錢
            /// 3	和局 輸贏金額 0 且 有效金額 0
            /// 4	進行中 非同步注單不會有進行中的單
            /// 6	取消單	
            /// 7	改單
            /// </summary>
            public int pre_Status { get; set; }
        }

        /// <summary>
        /// 注區下注列表 陣列
        /// </summary>
        public class BetType
        {
            /// <summary>
            /// 賠率
            /// </summary>
            public decimal odds { get; set; }

            /// <summary>
            /// 注區 ID
            /// </summary>
            public int spotId { get; set; }

            /// <summary>
            /// 注區代碼
            /// </summary>
            public string spotName { get; set; }

            /// <summary>
            /// 該注區下注金額總和
            /// </summary>
            public decimal betAmount { get; set; }

            /// <summary>
            /// 該注區輸贏金額總和
            /// </summary>
            public decimal loseWinAmount { get; set; }
        }

        /// <summary>
        /// 注區下注列表 陣列
        /// </summary>
        public class Transactions
        {
            /// <summary>
            /// 單注 ID
            /// </summary>
            public string transactionId { get; set; }

            /// <summary>
            /// 注區 ID
            /// </summary>
            public int spotId { get; set; }

            /// <summary>
            /// 注區代碼
            /// </summary>
            public string spotName { get; set; }

            /// <summary>
            /// 下注金額
            /// </summary>
            public decimal betAmount { get; set; }

            /// <summary>
            /// 輸贏金額
            /// </summary>
            public decimal payoffAmount { get; set; }
        }
    }
}
