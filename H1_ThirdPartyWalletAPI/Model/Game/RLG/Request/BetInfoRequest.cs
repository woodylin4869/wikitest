namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Request
{
    /// <summary>
    /// 注單資訊
    /// </summary>
    public class BetInfoRequest
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        public string SystemCode { get; set; }
        /// <summary>
        /// 站台代碼，即代理唯一識別碼 ID
        /// </summary>
        public string WebId { get; set; }
        /// <summary>
        /// 注單編號
        /// </summary>
        public string BetNo { get; set; }
        /// <summary>
        /// 語系
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// 回傳方式(0:json ，1:畫面)
        /// </summary>
        public int SetOption { get; set; }
    }
}
