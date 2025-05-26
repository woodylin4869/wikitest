namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Response
{
    /// <summary>
    /// XG 共用回傳欄位
    /// </summary>
    public class BaseResponse
    {
        /// <summary>
        /// 錯誤代碼, 0 代表沒錯誤
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// response 編號
        /// </summary>
        public string uuquid { get; set; }
    }
}
