namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Response
{
    // 回傳 共同欄位
    public class BaseDataResponse
    {
        /// <summary>
        /// BTi 错码 errorCode
        /// </summary>
        public int errorCode { get; set; }

        /// <summary>
        /// BTi 错码 errorMessage
        /// 有的方法有回 有的沒有
        /// </summary>
        public string? errorMessage { get; set; }
    }
}
