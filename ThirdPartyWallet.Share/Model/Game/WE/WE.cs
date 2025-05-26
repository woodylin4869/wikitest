using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE;
public class WE
{
    // 遊戲商支援的玩家語系
    public static Dictionary<string, string> Lang = new Dictionary<string, string>()
    {
        {"zh-CN", "cn"}, // 簡中
        {"zh-TW", "zh"}, // 繁中
        {"en-US", "en"}, // 英文
        {"vi-VN", "vi"}, // 越南
        {"ko-KR", "ko"}, // 韓文
        {"hi-IN", "in"}, // 印度文
        {"id-ID", "id"}, // 印尼文
        {"ja-JP", "ja"}, // 日文
        {"my-MM", "my"}, // 緬甸文
        {"th-TH", "th"}, // 泰文
        {"pt-PT", "pt"}, // 葡萄牙文
        {"es-ES", "es"}, // 西班牙文
    };

    /// <summary>
    /// H1 開放的幣別
    /// WE真人(廠商支援幣別有人民幣、泰銖、菲律賓幣、越千盾)
    /// </summary>
    public static Dictionary<string, string> Currency = new Dictionary<string, string>()
    {
        {"THB","THB"},  // 泰銖
    };

    /// <summary>
    /// 錯誤代碼
    /// 廠商無實做
    /// </summary>
    public static Dictionary<int, string> ErrorCode = new Dictionary<int, string>()
    {
        // {11000,"未指定的錯誤"},
    };



    public static string[] GetGamelist()
    {
        string[] GameList = new string[] { "星光百家樂 2", "新葡京廳 1", "菲律賓廳 1", "財神百家樂", "咪牌百家樂 8", "中國紅 1", "傳統百家樂 1", "菲律賓廳 3", "彩虹幸運輪", "番攤 1", "牛牛 75", "炸金花 1" };
        return GameList;
    }

    public static Dictionary<string, string> GroupGametype = new Dictionary<string, string>()
    {
        {"BA","BA"},
        {"FAN","FAN"},
        {"LW","LW"},
        {"OX","OX"},
        {"ZJH","ZJH"}
     };
}
