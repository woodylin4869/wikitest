namespace ThirdPartyWallet.Share.Model.Game.VA.Request
{
    public class ReportCurrencyRequest
    {
        /// <summary>
        /// 渠道編號 (若無渠道，請帶入0)
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// 搜尋使用時間 (0:下注時間)
        /// </summary>
        public int SearchMode { get; set; }

        /// <summary>
        /// 搜尋起始時間 (時區、格式請參考API注意事項)
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// 搜尋結束時間 (時區、格式請參考API注意事項)
        /// </summary>
        public string EndTime { get; set; }
    }
}
