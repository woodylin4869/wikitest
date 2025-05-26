namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Response
{
    /// <summary>
    /// 剔除遊戲中的會員
    /// </summary>
    public class KickAllResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int MsgID { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public long Timestamp { get; set; }
    }
}