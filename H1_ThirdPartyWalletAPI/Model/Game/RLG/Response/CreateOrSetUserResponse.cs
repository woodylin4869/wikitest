namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    /// <summary>
    /// 建立與更新會員
    /// </summary>
    public class CreateOrSetUserResponse
    {
        /// <summary>
        /// 000000 即為成功，其它代碼皆為失敗，可參詳 ErrorMessage 欄位內容
        /// </summary>
        public int errorcode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string errormessage { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// 回傳資料
        /// </summary>
        public string data { get; set; }

    }
}
