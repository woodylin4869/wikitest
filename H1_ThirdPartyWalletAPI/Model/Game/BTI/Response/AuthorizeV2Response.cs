namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Response
{
    /// <summary>
    /// 回傳 获取接口验证令牌
    /// </summary>
    public class AuthorizeV2Response : BaseDataResponse
    {
        /// <summary>
        /// 令牌
        /// </summary>
        public string? token { get; set; }
    }
}
