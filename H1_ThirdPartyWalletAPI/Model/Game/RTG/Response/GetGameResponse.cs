using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Response
{
    /// <summary>
    /// 取得遊戲列表
    /// </summary>
    public class GetGameResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int MsgID { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; }
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
            public List<GameInfo> Content { get; set; }
        }

        public class GameInfo
        {
            /// <summary>
            /// 遊戲ID
            /// </summary>
            public int GameId { get; set; }
            /// <summary>
            /// 遊戲名稱
            /// </summary>
            public string GameName { get; set; }
            /// <summary>
            /// 遊戲狀態
            /// 1:開放
            /// 2:維護
            /// </summary>
            public int GameStatus { get; set; }
        }
    }
}