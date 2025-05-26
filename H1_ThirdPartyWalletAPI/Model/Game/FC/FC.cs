using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC
{
    public class FC
    {
        /// <summary>
        /// 語系 Mapping
        /// key: W1, Value: 遊戲商
        /// </summary>
        public static Dictionary<string, int> lang = new Dictionary<string, int>()
        {
            {"zh-CN", 2},  // 簡體中文
            {"zh-TW", 2},  // 繁體中文
            {"en-US", 1},  // 英文
            {"th-TH", 4},  // 泰文
            {"id-ID", 5},  // 印尼文
            {"vi-VN", 3},  // 越南文
            {"my-MM", 6},  // 緬甸
            {"ja-JP", 7},  // 日文
            {"ko-KR", 8},  // 韓文
            //{"hi-IN", "hi-IN"},  // 印度
             //{"ta-IN", "ta-IN"},  // 印度 – 坦米爾
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖
       
        };
    }
}