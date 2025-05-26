using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.META.Response
{
    /// <summary>
    /// 取得遊戲帳務
    /// </summary>
    public class BetOrderRecordResponse : GetMetaDataDecryptBase
    {
        /// <summary>
        /// 總比數
        /// </summary>
        public int totalRows { get; set; }

        /// <summary>
        /// 剩餘筆數
        /// </summary>
        public int overRows { get; set; }
        /// <summary>
        /// 查詢筆數
        /// </summary>
        public int limit { get; set; }
        /// <summary>
        /// 資料列
        /// </summary>
        public List<Record> rows { get; set; }
    }

    public class Record
    {
        public Guid summary_id { get; set; }
        /// <summary>
        /// 流水編號
        /// </summary>
        [MaxLength(11)]
        public string Serial { get; set; }
        /// <summary>
        /// 注單號
        /// </summary>
        [MaxLength(50)]
        public string No { get; set; }
        /// <summary>
        /// 局號
        /// </summary>
        [MaxLength(255)]
        public string Round { get; set; }
        /// <summary>
        /// 下注數量
        /// </summary>
        public string BetCount { get; set; }
        /// <summary>
        /// 會員帳號
        /// </summary>
        [MaxLength(50)]
        public string Account { get; set; }
        /// <summary>
        /// 總下注金額 12+小數 4
        /// </summary>
        public int BetTotal { get; set; }
        /// <summary>
        /// 輸嬴金額 12+小數 4
        /// </summary>
        public int Winnings { get; set; }
        /// <summary>
        /// 是否已對獎 0:未對獎 1:已對獎
        /// </summary>
        public string Collect { get; set; }
        /// <summary>
        /// 是否已對獎 1：取消 2：未開獎 3：未中獎 4：中獎 5：和局
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 本期所屬日期
        /// </summary>
        public DateTime DateCurrent { get; set; }
        /// <summary>
        /// 封盤時間
        /// </summary>
        public DateTime DateClosing { get; set; }
        /// <summary>
        /// 開獎時間
        /// </summary>
        public DateTime DateDraw { get; set; }
        /// <summary>
        /// 下注時間
        /// </summary>
        public DateTime DateCreate { get; set; }
        /// <summary>
        /// 遊戲類別
        /// </summary>
        public string GameType { get; set; }
        /// <summary>
        /// 遊戲名稱
        /// </summary>
        public string Game { get; set; }
        /// <summary>
        /// 遊戲桌名稱
        /// </summary>
        [MaxLength(50)]
        public string Table { get; set; }
        /// <summary>
        /// 桌號
        /// </summary>
        public string TableId { get; set; }
        /// <summary>
        /// 幣別編號
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 匯率
        /// </summary>
        [MaxLength(11)]
        public string Rate { get; set; }
        public Details[] Detail { get; set; }
        public class Details
        {
            public string Content { get; set; }
            public int Price { get; set; }
        }

    }
}