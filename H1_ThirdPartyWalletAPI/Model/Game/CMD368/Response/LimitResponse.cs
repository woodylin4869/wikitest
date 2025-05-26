namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    public class LimitResponse
    {
        /// <summary>
        /// 操作批次号
        /// </summary>
        public string SerialKey { get; set; }
        /// <summary>
        /// 操作批次时间
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 訊息說明
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 不同的方法返回不同的数据
        /// </summary>
        public object Data { get; set; }
    }


}

