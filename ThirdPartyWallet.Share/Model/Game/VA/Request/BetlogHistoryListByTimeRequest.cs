namespace ThirdPartyWallet.Share.Model.Game.VA.Request
{
    public class BetlogHistoryListByTimeRequest
    {
        /// <summary>
        /// 渠道編號 (若無渠道，請帶入0)
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// 搜尋使用模式 (0: 下注時間)
        /// </summary>
        public int SearchMode { get; set; }

        /// <summary>
        /// 搜尋起始時間 (時區、格式請參考API注意事項)
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 搜尋結束時間 (時區、格式請參考API注意事項)
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 查詢頁數 (最小1頁)
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每頁顯示筆數 (最小2000筆，最大5000筆)
        /// </summary>
        public int PageSize { get; set; }
    }
}
