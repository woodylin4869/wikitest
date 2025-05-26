using System.Collections.Generic;

namespace ThirdPartyWallet.Share.Model.Game.Gemini
{
    public class Gemini
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
            {11000,"未指定的錯誤"},
            {11001,"無效的令牌"},
            {11002,"令牌已過期"},
            {11003,"認證憑證不正確"},
            {11004,"參數不正確"},
            {11005,"重複的請求"},
            {11006,"序列不存在"},
            {11007,"不允許的操作"},
            {11008,"重複的交易單號"},
            {11009,"不存在的交易單號"},
            {12001,"登入失敗"},
            {12002,"帳戶已鎖定"},
            {12003,"帳戶不存在"},
            {12004,"帳戶在黑名單上"},
            {12005,"帳戶已存在"},
            {13001,"玩家資金不足"},
            {13002,"無效的 IP 地址"},
            {13003,"超時"},
            {14001,"訂單不存在"},
            {15001,"維護"},

        };
    }
}
