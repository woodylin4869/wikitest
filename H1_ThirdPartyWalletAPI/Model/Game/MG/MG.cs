using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG
{
    public class MG
    {
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en-US"},
            {"th-TH", "th-TH"},
            {"vi-VN", "vi-VN"},
            {"zh-TW", "zh-CN"}, //繁中可支援遊戲較少, 直接使用簡中
            {"zh-CN", "zh-CN"},
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB", "THB"},
        };
    }
}
