using System.Collections.Generic;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH
{
    public class RGRICH
    {
        // 遊戲商支援的玩家語系
        public static Dictionary<string, string> Lang = new Dictionary<string, string>()
        {
            {"en-US", "en"},    // 英文
            {"zh-CN", "zh_CN"}, // 簡中
            {"zh-TW", "zh_TW"}, // 繁中
            {"vi-VN", "vi"},    // 越南
            {"th-TH", "th"},    // 泰文
        };

        /// <summary>
        /// H1 開放的幣別
        /// RG富遊 (廠商支援幣別有人民幣、泰銖、菲律賓幣、越千盾)
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
    }
}