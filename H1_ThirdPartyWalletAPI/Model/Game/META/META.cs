using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.META
{
    public class META
    {
        /// <summary>
        /// 語系 Mapping
        /// key: W1, Value: 遊戲商
        /// </summary>
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            //{"zh-CN", "zh-CN"},  // 簡體中文
            {"zh-TW", "zh-TW"},  // 繁體中文
            {"en-US", "en"},  // 英文
            //{"th-TH", "th-TH"},  // 泰文
             //{"id-ID", "id-ID"},  // 印尼文
             //{"vi-VN", "vi-VN"},  // 越南文
             //{"my-MM", "my-MM"},  // 緬甸
            // {"ja-JP", "ja-JP"},  // 日文
            // {"hi-IN", "hi-IN"},  // 印度
             //{"ta-IN", "ta-IN"},  // 印度 – 坦米爾
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","8"},  // 泰銖
            //{"RMB","2"},  // 人民幣
            //{"USD","3"},  // 美元
            //{"TWD","1"},   // 新台幣
            //{"MYR","5"},  // 馬幣
            //{"PHP","4"},  // 披索
            //{"MMK","MMK"},  // 緬甸緬元
         
            //{"KRW","KRW"},  // 韓圓
            //{"JPY","JPY"},  // 日圓
         
            //{"HKD","HK"},   // 港元
            //{"INR","INR"},  // 印度盧比
            //{"SGD","SGD"},  // 新加坡元

            
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