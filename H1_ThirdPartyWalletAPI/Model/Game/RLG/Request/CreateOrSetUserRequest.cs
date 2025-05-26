namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Request
{
    /// <summary>
    /// 建立與更新會員
    /// </summary>
    public class CreateOrSetUserRequest
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
        /// 玩家的唯一識別碼
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 玩家顯示名稱，4~16 字元，可使用英文字母、數字等字元
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 語系
        /// </summary>
        public string Language { get; set; }
    }
}
