using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG
{
    public class XG
    {
        /// <summary>
        /// api 語系, 參數名改用 lang 亦可 (W1轉XG)
        /// default: zh-CN 
        /// Enum: [zh-CN, zh-TW, en-US]
        /// </summary>
        public static Dictionary<string, string> ApiLang = new Dictionary<string, string>()
        {
            {"en-US", "en-US"},
            {"th-TH", "en-US"},
            {"vi-VN", "en-US"},
            {"zh-TW", "zh-TW"},
            {"zh-CN", "zh-CN"},
            {"id-ID", "en-US"},
        };

        /// <summary>
        /// 遊戲語系 (W1轉XG)
        /// https://github.com/jacky5823a/docs/blob/master/AccountingPlatformAPI/reference-cht.md#%E9%81%8A%E6%88%B2%E8%AA%9E%E7%B3%BB
        /// zh-CN 簡體
        /// zh-TW 繁體
        /// en / en-US 英語
        /// th 泰語
        /// id 印尼文
        /// ko 韩文
        /// ja 日語
        /// vi / vn 越語
        /// ms 馬來語
        /// </summary>
        public static Dictionary<string, string> GameLang = new Dictionary<string, string>()
        {
            {"zh-CN", "zh-CN"},
            {"zh-TW", "zh-TW"},
            {"en-US", "en-US"},
            {"th-TH", "th"},
            {"id-ID", "id"},
            {"vi-VN", "vn"},
        };

        /// <summary>
        /// 此幣別需該代理有啟用才能使用
        /// 支援幣別請參考下表，幣別代碼依照 ISO_4217 制定
        /// https://github.com/jacky5823a/docs/blob/master/AccountingPlatformAPI/XG/accounting-platform-cht.md#%E6%B3%A8%E6%84%8F%E4%BA%8B%E9%A0%85
        /// </summary>
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB", "THB"},  // 泰銖
            {"USD", "USD"},  // 美金
        };
    }
}
