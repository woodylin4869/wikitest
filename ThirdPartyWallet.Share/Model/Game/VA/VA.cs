namespace ThirdPartyWallet.Share.Model.Game.VA
{
    public class VA
    {
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
            //{"ms-MY", "ms-MY"},
            //{"ru-RU", "ru-RU"},
            {"pt-BR", "pt-BR"},
            //{"es-SP", "es-SP"},
            //{"tr-TR", "tr-TR"},
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



        /// <summary>
        /// 裝置
        /// </summary>
        public static Dictionary<string, string> Device = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
             {"DESKTOP","web"},  //PC
             {"MOBILE", "mobile"}  //MOBILE
        };

        public static Dictionary<int, string> ErrorCode = new Dictionary<int, string>()
        {
            { 0, "Success" },
            { 101, "API config error." },
            { 102, "Sign invalid." },
            { 103, "Api failed." },
            { 104, "Under maintenance." },
            { 105, "IP not allowed to access api." },
            { 106, "Api timeout." },
            { 201, "Bad parameters." },
            { 202, "Account duplicated." },
            { 203, "Transactionid duplicated." },
            { 204, "Insufficient balance." },
            { 205, "Account does not exist." },
            { 206, "Game does not exist." },
            { 429, "Calls Api too frequently, please try again later." },
            { 601, "Bet already exists." },
            { 602, "Bet was not found." },
            { 603, "Bet was already settled." },
            { 999, "Something wrong." }
        };

    }
}
