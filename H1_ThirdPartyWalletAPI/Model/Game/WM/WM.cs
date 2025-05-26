using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.WM
{
    public class WM
    {
        public static Dictionary<string, int> lang = new Dictionary<string, int> ()
        {
            {"zh-CN",0 },//簡體中文
            {"en-US",1},//英文
            {"th-TH",2},//泰文
            {"vi-VN",3},//越文
            {"ja-JP",4},//日文
            {"ko-KR",5},//韓文
            {"hi-IN",6},//印度
            {"ms-MY",7},//馬來西亞
            {"id-ID",8},//印尼
            {"zh-TW",9},//繁體
        };

        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖
            {"USD","USD"},  //美金
        };

    }
}
