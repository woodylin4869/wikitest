using System.Collections.Generic;

namespace ThirdPartyWallet.Share.Model.Game.PS
{
    public class Ps
    {
        // todo: 此列表現況無用 GR 沒提供可選語系列表
        // GR 回應: 可透過代理後台 預設代理底下玩家語系
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en-US"},
            {"zh-CN", "zh-CN"},
            {"zh-TW", "zh-TW"},
            {"vi-VN", "vi-VN"},
            {"th-TH", "th-TH"},
            {"ja-JP", "ja-JP"},
            {"ko-KR", "ko-KR"},
            {"id-ID", "id-ID"},
            {"ms-MY", "ms-MY"},
            {"ru-RU", "ru-RU"},
            {"pt-BR", "pt-BR"},
            {"es-SP", "es-SP"},
            {"tr-TR", "tr-TR"},
        };

        // todo: 此列表現況無用 GR 沒提供可選幣別列表
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
        public static Dictionary<int, string> ErrorCode = new Dictionary<int, string>()
        {
            {0,""}
        };
    }
}
