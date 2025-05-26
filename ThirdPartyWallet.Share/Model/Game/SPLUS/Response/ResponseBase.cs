namespace ThirdPartyWallet.Share.Model.Game.SPLUS.Response
{
    public class ResponseBase<T>
    {
        /// <summary>
        /// 資料
        /// </summary>
        public T data { get; set; }
        /// <summary>
        /// 狀態
        /// </summary>
        public Status status { get; set; }

        public class Status
        {
            /// <summary>
            /// 狀態碼
            /// </summary>
            public string code { get; set; }
            /// <summary>
            /// 狀態訊息
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// 回應時間 UNIX時間戳
            /// </summary>
            public long timestamp { get; set; }
        }
    }
}
