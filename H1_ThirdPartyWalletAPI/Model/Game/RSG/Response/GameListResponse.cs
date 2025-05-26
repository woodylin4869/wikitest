using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 取得遊戲列表
    /// </summary>
    public class GameListResponse
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
            public List<GameInfo> GameList { get; set; }
        }

        public class GameInfo
        {
            /// <summary>
            /// 遊戲代碼
            /// </summary>
            public int GameId { get; set; }
            /// <summary>
            /// 遊戲類型(1:老虎機 2:捕魚機)
            /// </summary>
            public int GameType { get; set; }
            public GameName GameName { get; set; }
            /// <summary>
            /// 滾輪規格
            /// </summary>
            public string RollerSpec { get; set; }
            /// <summary>
            /// 連線類型
            /// </summary>
            public string LineType { get; set; }
            /// <summary>
            /// 遊戲狀態(1:正常 2:維護)
            /// </summary>
            public int GameStatus { get; set; }
            /// <summary>
            /// 遊戲圖片網址(改由其它方式提供)
            /// </summary>
            public string GamePicUrl { get; set; }
            /// <summary>
            /// 遊戲資源網址(改由其它方式提供)
            /// </summary>
            public string GameResUrl { get; set; }
            /// <summary>
            /// 連線數
            /// </summary>
            public string LineNumber { get; set; }
        }

        public class GameName
        {
            /// <summary>
            /// 遊戲名稱(英文)
            /// </summary>
            public string en_US { get; set; }
            /// <summary>
            /// 遊戲名稱(繁體中文
            /// </summary>
            public string zh_TW { get; set; }
            /// <summary>
            /// 遊戲名稱(簡體中文)
            /// </summary>
            public string zh_CN { get; set; }
            /// <summary>
            /// 遊戲名稱(泰文)
            /// </summary>
            public string th_TH { get; set; }
            /// <summary>
            /// 遊戲名稱(韓文)
            /// </summary>
            public string ko_KR { get; set; }
            /// <summary>
            /// 遊戲名稱(日文)
            /// </summary>
            public string ja_JP { get; set; }
            /// <summary>
            /// 遊戲名稱(緬甸文)
            /// </summary>
            public string en_MY { get; set; }
            /// <summary>
            /// 遊戲名稱(印尼文)
            /// </summary>
            public string id_ID { get; set; }
        }
    }
}