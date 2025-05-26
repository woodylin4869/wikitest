using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// ( 老虎機專用 ) 取得特定遊戲紀錄的遊戲盤面網址
    /// </summary>
    public class GetGameRecordURLResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// API 呼叫回傳的 JSON 格式的 object / object array
        /// </summary>
        public DataInfo Data { get; set; }

        public class DataInfo
        {
            public string URL { get; set; }
        }
    }
}