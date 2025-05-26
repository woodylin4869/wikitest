namespace ThirdPartyWallet.Share.Model.Game.VA.Response
{
    public class BetlogDetailResponse
    {
        /// <summary>
        /// 注單編號
        /// </summary>
        public string BetId { get; set; }

        /// <summary>
        /// 下注時間 (時區、格式請參考API注意事項)
        /// </summary>
        public DateTime BetTime { get; set; }

        /// <summary>
        /// 成單時間 (時區、格式請參考API注意事項)
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 注單詳細資訊頁面連結
        /// </summary>
        public string Url { get; set; }
    }
}
