using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY
{
    public class SEXY
    {
        /// <summary>
        /// 語系 Mapping
        /// key: W1, Value: 遊戲商
        /// </summary>
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"zh-CN", "cn"},  // 簡體中文
            {"zh-TW", "cn"},  // 繁體中文
            {"en-US", "en"},  // 英文
            {"th-TH", "th"},  // 泰文
             //{"id-ID", "id-ID"},  // 印尼文
            {"vi-VN", "vn"},  // 越南文
             //{"my-MM", "my-MM"},  // 緬甸
            {"ja-JP", "jp"},  // 日文
             // {"hi-IN", "hi-IN"},  // 印度
             //{"ta-IN", "ta-IN"},  // 印度 – 坦米爾
            {"ko-kr", "kr"},  // 韓文
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖
            //{"RMB","CNY"},  // 人民幣
            //{"USD","USD"},  // 美元
            //{"PHP","PHP"},  // 披索
            //{"MMK","MMK"},  // 緬甸緬元
            //{"KRW","KRW"},  // 韓圓
            //{"JPY","JPY"},  // 日圓
            //{"HKD","HKD"},   // 港元
            //{"INR","INR"},  // 印度盧比
            //{"SGD","SGD"},  // 新加坡元
            //{"VND","VND"},  // 越南盾(1:1000)
           // {"IDR","IDR"},  // 印尼盾(1:1000)
            // W1 不支援
            //{"","RMB"},  // 人民幣
            //{"","EUR"},  // 歐元
            //{"","GBP"},  // 英鎊
            //{"","USDT"}, // 泰達幣
            //{"","MYR2"}, // 馬幣
        };

        public static List<string> SexyBetLimitList = new List<string>()
        {
           "260901",
           "260902",
           "260903",
           "260904",
           "260905",
           "260906",
           "260907",
           "260908",
           "260912",
           "260913",
           "260914",
           "260915",
           "260916",
           "260923",
           "260924",
           "260926",
           "260927",
        };

        public class BetLimitClass
        {
            public SEXYBCRT SEXYBCRT { get; set; }
        }

        public class SEXYBCRT
        {
            public LIVE LIVE { get; set; }
        }

        public class LIVE
        {
            public List<int> limitId { get; set; }
        }



        public static class LiveGameMap
        {
            public static readonly Dictionary<string, int> CodeToId = new()
            {
                {"MX-LIVE-001", 1},
                {"MX-LIVE-002", 2},
                {"MX-LIVE-003", 3},
                {"MX-LIVE-005", 5},
                {"MX-LIVE-006", 6},
                {"MX-LIVE-007", 7},
                {"MX-LIVE-009", 9},
                {"MX-LIVE-010", 10},
                {"MX-LIVE-012", 12},
                {"MX-LIVE-014", 14},
                {"MX-LIVE-015", 15}
            };

            public static readonly Dictionary<string, string> IdToCode = new()
            {
                {"1", "MX-LIVE-001"},
                {"2", "MX-LIVE-002"},
                {"3", "MX-LIVE-003"},
                {"5", "MX-LIVE-005"},
                {"6", "MX-LIVE-006"},
                {"7", "MX-LIVE-007"},
                {"9", "MX-LIVE-009"},
                {"10","MX-LIVE-010"},
                {"13","MX-LIVE-012"},
                {"14","MX-LIVE-014"},
                {"15","MX-LIVE-015"}
            };
        }
    }
}