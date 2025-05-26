namespace ThirdPartyWallet.Share.Model.Game.CR
{
    public class CR
    {
        // 遊戲商支援的玩家語系
        public static Dictionary<string, string> Lang = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"en-US", "en-us"}, // 英文
            {"zh-CN", "zh-cn"}, // 簡中
            {"zh-TW", "zh-tw"}, // 繁中
            {"id-ID", "id-id"}, // 印尼文 
            {"ja-JP", "ja-jp"}, // 日語
            {"th-TH", "th-th"}, // 泰文
            //{"hi-IN", "sa-in"}, // 印度文
            {"vi-VN", "vi-vn"},    // 越南
            {"ko-kr", "ko-kr"}, // 韓文
            //{"km-kh", "km-kh"}, // 柬埔寨文
            {"my-MM", "my-mm"},    // 緬甸文 
            {"es-ES", "es-es"},    // 西班牙文 
        };

        /// <summary>
        /// H1 開放的幣別
        /// </summary>
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
             {"THB","THB"},  // 泰銖
        };

        /// <summary>
        /// 裝置
        /// </summary>
        public static Dictionary<string, string> Device = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
             {"DESKTOP","PC"},  //PC
             {"MOBILE", "MOBILE"}  //MOBILE
        };

        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public static Dictionary<string, string> ErrorCode = new Dictionary<string, string>()
        {
             {"0000", "成功"},
             {"0001", "參數錯誤"},
             {"0002", "Token驗證錯誤"},
             {"0003", "查無資料"},
             {"0004", "信用額度用戶不支持存提款功能"},
             {"0005", "上層餘額不足，無法存入"},
             {"0006", "餘額不足，無法提出"},
             {"0007", "不可新增會員帳號"},
             {"0008", "輸入的帳號已經有人使用"},
             {"0011", "找不到 AID 的資料物件"},
             {"0012", "编解码發生錯誤 (解Request)"},
             {"0013", "编解码發生錯誤 (编Response)"},
             {"0014", "未被定義的方法"},
             {"0015", "人數已滿"},
             {"0016", "系統維護中"},
             {"0017", "系統流量較高 請重新再試"}
        };
    }
}