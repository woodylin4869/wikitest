using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG
{
    public class RLG
    {
        /// <summary>
        /// 語系
        /// </summary>
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"th-TH", "thai"},  // 泰文
            {"en-US", "en"}, // 英文 
            {"zh-TW", "zhtw"},  // 繁體中文
            {"zh-CN", "zhcn"},  // 簡體中文
        };
        /// <summary>
        /// 幣別
        /// </summary>
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖

        };
    }
}
