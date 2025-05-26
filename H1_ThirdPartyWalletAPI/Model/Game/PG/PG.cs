using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game.PG
{
    public class PG
    {
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en"},
            {"th-TH", "th"},
            {"vi-VN", "vi"},
            {"zh-TW", "zh"},
            {"zh-CN", "zh"},
            {"ko-KR", "ko"}
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB", "THB"},
        };
    }
}
