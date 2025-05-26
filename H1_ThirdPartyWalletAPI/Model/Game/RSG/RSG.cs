using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG
{
    public class RSG
    {
        /// <summary>
        /// 語系 Mapping
        /// key: W1, Value: 遊戲商
        /// </summary>
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en-US"},  // 英文
            {"zh-TW", "zh-TW"},  // 繁體中文
            {"zh-CN", "zh-CN"},  // 簡體中文
            {"th-TH", "th-TH"},  // 泰文
            {"ko-KR", "ko-KR"},  // 韓文
            {"my-MM", "en-MY"},  // 緬甸文
            {"vi-VN", "vi-VN"},  // 越南文
            {"ja-JP", "ja-JP"},  // 日文
            {"id-ID", "id-ID"},  // 印尼文
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖
            {"TWD","NT"},   // 新台幣
            {"MMK","MMK"},  // 緬甸緬元
            {"RMB","RMB"},  // 人民幣
            {"USD","USA"},  // 美元
            {"KRW","KRW"},  // 韓圓
            {"JPY","JPY"},  // 日圓
            {"MYR","MYR"},  // 馬幣
            {"HKD","HK"},   // 港元
            {"INR","INR"},  // 印度盧比
            {"SGD","SGD"},  // 新加坡元
            {"PHP","PHP"},  // 披索
            {"VND","VND"},  // 越南盾
            {"IDR","IDR"},  // 印尼盾
            
            // W1 不支援
            //{"","RMB"},  // 人民幣
            //{"","EUR"},  // 歐元
            //{"","GBP"},  // 英鎊
            //{"","USDT"}, // 泰達幣
            //{"","MYR2"}, // 馬幣
        };
    }
}