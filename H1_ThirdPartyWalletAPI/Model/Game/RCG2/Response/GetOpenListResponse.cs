using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RCG2.Response
{
    /// <summary>
    /// 開牌紀錄 /api/Record/GetOpenList
    ///
    /// 用途說明與限制
    /// 用於查詢遊戲桌別開牌紀錄。
    /// 最小精度為單一輪局。
    /// 每日中午12點前的開牌紀錄查詢時間為前一天。
    /// 舉例： 若今天日期為2022-08-25，愈查詢今日10點後開牌紀錄，查詢時間請輸入2022-08-24。
    /// 
    /// 補充說明
    /// GameDeskID請參考 取得遊戲桌別資訊 的data/dataList/id。
    /// Date查詢的單位為日 如輸入2022-08-25或2022-08-25 08:00:00，皆代表查詢 2022-08-24 12:00:00 ~ 2022-08-25 11:59:59。
    /// </summary>
    public class GetOpenListResponse
    {
        public List<Openlist> dataList { get; set; }
    }

    public class Openlist
    {
        /// <summary>
        /// 輪號
        /// </summary>
        public string activeNo { get; set; }

        /// <summary>
        /// 局號
        /// </summary>
        public string runNo { get; set; }

        /// <summary>
        /// 開牌結果 請參考 遊戲開牌結果說明
        /// </summary>
        public string result { get; set; }

        /// <summary>
        /// 提供開牌結果的圖示連結
        /// </summary>
        public string url { get; set; }
    }
}
