using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI
{
    public class JILI
    {
        /// <summary>
        /// 語系 Mapping
        /// key: W1, Value: 遊戲商
        /// </summary>
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"zh-CN", "zh-CN"},  // 簡體中文
            {"zh-TW", "zh-TW"},  // 繁體中文
            {"en-US", "en-US"},  // 英文
            {"th-TH", "th-TH"},  // 泰文
            {"id-ID", "id-ID"},  // 印尼文
            {"vi-VN", "vi-VN"},  // 越南文
            {"my-MM", "my-MM"},  // 緬甸
            {"ja-JP", "ja-JP"},  // 日文
            {"hi-IN", "hi-IN"},  // 印度
            {"ta-IN", "ta-IN"},  // 印度 – 坦米爾
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖
            //{"TWD","NT"},   // 新台幣
            //{"MMK","MMK"},  // 緬甸緬元
            //{"RMB","RMB"},  // 人民幣
            //{"USD","USA"},  // 美元
            //{"KRW","KRW"},  // 韓圓
            //{"JPY","JPY"},  // 日圓
            //{"MYR","MYR"},  // 馬幣
            //{"HKD","HK"},   // 港元
            //{"INR","INR"},  // 印度盧比
            //{"SGD","SGD"},  // 新加坡元
            //{"PHP","PHP"},  // 披索
            
            // W1 不支援
            //{"VND","VND"},  // 越南盾
            //{"IDR","IDR"},  // 印尼盾
            //{"","RMB"},  // 人民幣
            //{"","EUR"},  // 歐元
            //{"","GBP"},  // 英鎊
            //{"","USDT"}, // 泰達幣
            //{"","MYR2"}, // 馬幣
        };
    }
}