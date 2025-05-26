namespace ThirdPartyWallet.Share.Model.Game.VA.Response
{
    public class BetlogListByTimeResponse
    {
        /// <summary>
        /// 注單列表
        /// </summary>
        public List<Betlog> BetlogList { get; set; }

        /// <summary>
        /// 當前頁數
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 注單啟始處 (第n筆)
        /// </summary>
        public int From { get; set; }

        /// <summary>
        /// 注單結束處 (第n筆)
        /// </summary>
        public int To { get; set; }

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PerPage { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int LastPage { get; set; }

        /// <summary>
        /// 搜尋區間內總注單數
        /// </summary>
        public int Total { get; set; }
    }


    public class Betlog
    {
        /// <summary>
        /// 注單version key
        /// </summary>
        public long VersionKey { get; set; }

        /// <summary>
        /// 注單單號 (注單唯一值)
        /// </summary>
        public string BetId { get; set; }

        /// <summary>
        /// 渠道編號
        /// </summary>
        public int ChannelId { get; set; }

        /// <summary>
        /// 會員帳號
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// (請洽詢我方商務)
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 遊戲編號
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// 下注金額
        /// </summary>
        public decimal Bet { get; set; }

        /// <summary>
        /// 派彩金額
        /// </summary>
        public decimal Payout { get; set; }

        /// <summary>
        /// 輸贏金額
        /// </summary>
        public decimal WinLose { get; set; }

        /// <summary>
        /// 是否包含免費遊戲 (0:不包含, 1:包含)
        /// </summary>
        public int FreeGame { get; set; }

        /// <summary>
        /// 注單狀態 (0:未派彩, 1:已派彩)
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 下注類型 (main:一般遊戲, special:特色遊戲, item:道具卡)
        /// </summary>
        public string BetMode { get; set; }

        /// <summary>
        /// 下注時間 (時區、格式請參考API注意事項)
        /// </summary>
        public DateTime BetTime { get; set; }

        /// <summary>
        /// 成單時間 (時區、格式請參考API注意事項)
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 結算時間 (時區、格式請參考API注意事項)
        /// </summary>
        public DateTime SettleTime { get; set; }


        #region DB_Model
        public DateTime report_time { get; set; }

        /// <summary>
        /// BetTime
        /// </summary>
        public DateTime partition_time { get; set; }


        /// <summary>
        /// 活動派彩金額
        /// </summary>
        public decimal jackpotwin { get; set; }
        #endregion


    }
}
