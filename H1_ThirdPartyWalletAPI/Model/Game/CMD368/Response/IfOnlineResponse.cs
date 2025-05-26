namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    /// <summary>
    /// 會員是否在線
    /// </summary>
    public class IfOnlineResponse
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
            public Date[] Data { get; set; }
        }

        public class Date
        {
        /// <summary>
        /// 用戶名稱
        /// </summary>
            public string UserName { get; set; }
        /// <summary>
        /// 在線：true ， 不在線:false
        /// </summary>
            public bool IsOnline { get; set; }
        }

    }

