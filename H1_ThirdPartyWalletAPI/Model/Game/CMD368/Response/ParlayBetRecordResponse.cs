namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    public class ParlayBetRecordResponse
    {
        /// <summary>
        /// 操作批次號
        /// </summary>
        public string SerialKey { get; set; }
        /// <summary>
        /// 操作批次時間
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// 操作是否成功資訊
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 消息說明
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 串關資訊
        /// </summary>
        public object[] Data { get; set; }
    }
}
