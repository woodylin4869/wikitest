using System.Collections.Generic;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot
{
    public class EGSlot
    {
        // todo: 此列表現況無用 GR 沒提供可選語系列表
        // GR 回應: 可透過代理後台 預設代理底下玩家語系
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en"},
            {"zh-CN", "zh-chs"},
            {"zh-TW", "zh-cht"},
            {"vi-VN", "vi"},
            {"th-TH", "th"},
            {"ja-JP", "ja"},
            {"ko-KR", "ko"},
            {"id-ID", "id"},
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
            {1,"注單已存在"},
            {2,"餘額不足"},
            {3,"驗證失敗或過期"},
            {4,"遊戲不存在"},
            {5,"加密驗證失敗"},
            {6,"紀錄不存在/玩家不存在/玩家不在線上"},
            {7,"格式驗證錯誤"},
            {8,"紀錄已存在 (ex: 代理已存在)"},
            {9,"請求路徑或方法錯誤"},
            {99,"其他錯誤"},

        };
    }
}
