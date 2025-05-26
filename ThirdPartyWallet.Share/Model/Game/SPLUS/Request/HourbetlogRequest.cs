namespace ThirdPartyWallet.Share.Model.Game.SPLUS.Request
{
    public class HourbetlogRequest
    {
        /// <summary>
        /// 搜尋起始時間(GMT+8)
        /// </summary>
        public string start_time { get; set; }
        /// <summary>
        /// 搜尋結束時間(GMT+8)
        /// </summary>
        public string end_time { get; set; }
        /// <summary>
        /// 玩家遊戲帳號  (忽略此欄位即返回全部用戶數據)
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 遊戲代碼
        /// </summary>
        public string gamecode { get; set; }
        /// <summary>
        /// 按照下列欄位數據分組:0=玩家遊戲帳號、遊戲代碼 1=遊戲代碼
        /// </summary>
        public string group_by { get; set; }
    }
}
