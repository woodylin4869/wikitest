namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    /// <summary>
    /// GR 共用回傳欄位
    /// </summary>
    public class GRResponseBase
    {
        /// <summary>
        /// 回傳狀態( Y 或 N)
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 錯誤代碼(無資料代表正常)
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 錯誤訊息(無資料代表正常)
        /// </summary>
        public string message { get; set; }
    }
}
