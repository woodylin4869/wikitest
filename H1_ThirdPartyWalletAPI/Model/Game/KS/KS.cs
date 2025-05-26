using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS
{
    public class KS
    {
        /// <summary>
        /// 語系 Mapping
        /// key: W1, Value: 遊戲商
        /// </summary>
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"zh-CN", "CHN"},  // 簡體中文
            {"zh-TW", "THN"},  // 繁體中文
            {"en-US", "ENG"},  // 英文
            {"th-TH", "TH"},  // 泰文
            {"id-ID", "ID"},  // 印尼文
            {"vi-VN", "VN"},  // 越南文
            {"ja-JP",  "JP"},  // 日文
            {"ko-KR",  "KR"},  // 韓文
            {"ru-RU", "RU"},  // 俄语
            {"fr-FR", "FR"},  // 法语
            {"de-DE", "DE"},  // 德语
            {"it-IT", "IT"},  // 意大利语
            //{"my-MM", ""},  // 緬甸
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖
        };
    }
}
