using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game
{
    /// <summary>
    ///
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
    public class RCG_GetOpenList
    {
        /// <summary>
        /// 遊戲桌別
        /// </summary>
        [Required]
        public string GameDeskID { get; set; }

        /// <summary>
        /// 輪號
        /// </summary>
        public string ActiveNo { get; set; }

        /// <summary>
        /// 局號
        /// </summary>
        public string RunNo { get; set; }

        /// <summary>
        /// 查詢日期 格式 yyyy-mm-dd
        /// </summary>
        [Required]
        public DateTime Date { get; set; }
    }
}
