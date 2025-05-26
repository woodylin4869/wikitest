using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER;

public class JOKER
{
    public static Dictionary<string, string> lang = new Dictionary<string, string>()
    {
        {"en-US", "en"},
        {"th-TH", "th"},
        {"zh-CN", "zh"},
        {"zh-TW", "zh"},
        {"id-ID", "id"},
    };

    public static Dictionary<string, string> GameDetaliLang = new Dictionary<string, string>()
    {
        {"en-US", "en"},
        {"zh-CN", "zh"},
        {"zh-TW", "zh"},
    };

    public static Dictionary<string, string> Currency = new Dictionary<string, string>()
    {
        {"THB", "THB"},
    };
}