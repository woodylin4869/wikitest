namespace H1_ThirdPartyWalletAPI.Middleware
{
    public class ScopedLogTemp
    {
        /// <summary>
        /// 縮短版的 response body 避免過大的 response log 進不了 gcp log
        /// </summary>
        public string ShortResponseBody { get; set; }
    }
}
