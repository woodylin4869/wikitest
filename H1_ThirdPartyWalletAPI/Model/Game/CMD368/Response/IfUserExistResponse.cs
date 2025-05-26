namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    /// <summary>
    /// 會員是否存在
    /// </summary>
    public class IfUserExistResponse
    {
        /// <summary>
        /// 操作批次號
        /// </summary>
        public string SerialKey { get; set; }
        /// <summary>
        /// 時間戳記
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
        /// 存在:true , 不存在:false
        /// </summary>
        public bool Data { get; set; }


    }
}
