namespace ThirdPartyWallet.Share.Model.Game.VA.Response
{
    public class BetlogHistoryListByTimeResponse
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
}
