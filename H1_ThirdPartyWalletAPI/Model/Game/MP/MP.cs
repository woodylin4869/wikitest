using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MP
{
    public class MP
    {
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US","en-us"},//英文
            {"zh-TW","zh-cn" },//繁體中文 
            {"zh-CN","zh-cn" },//簡體中文
            {"th-TH","th"},//泰文
            {"vi-VN","vie"},//越文
            {"id-ID","ind"},//印尼
            {"hi-IN","hi"},//印度
        };

        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖
        };

    }
}
