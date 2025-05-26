namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Request
{
    /// <summary>
    /// 查詢玩家在線列表
    /// </summary>
    public class PlayerOnlineListRequest
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
        /// 玩家的唯一識別碼 可空值
        /// </summary>
        public string? UserId { get; set; }
    }
}
