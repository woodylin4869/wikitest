using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    public class LanguageInfoResponse

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

        public Dictionary<string, string> Data { get; set; }
    }

}

