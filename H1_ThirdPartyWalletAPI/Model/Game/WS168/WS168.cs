using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.WS168
{
    public class WS168
    {
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US","en"},//英文
            {"zh-CN","cn" },//簡體中文
            {"id-ID","id"},//印尼
            {"th-TH","th"},//泰文
            {"vi-VN","vi"},//越文
            {"zh-TW","zh-TW"},//繁體
        };

        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖
        };


        public static Dictionary<string, int> CodeToId = new()
        {
            {"COCKFIGHT", 1},
        };
    }
}
