using System.Collections.Generic;
using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.RCG2
{
    public class RCG2
    {
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"RMB", "CNY"},
            //{"HKD", "HKD"},
            //{"KRW", "KRW"},
            //{"MYR", "MYR"},
            //{"SGD", "SGD"},
            //{"USD", "USD"},
            //{"JPY", "JPY"},
            //{"THB", "THB"},
            //{"IDR", "IDR"},
            //{"EUR", "EUR"},
            //{"GBP", "GBP"},
            //{"CHF", "CHF"},
            //{"MXN", "MXN"},
            //{"CAD", "CAD"},
            //{"RUB", "RUB"},
            //{"INR", "INR"},
            //{"RON", "RON"},
            //{"DKK", "DKK"},
            //{"NOK", "NOK"},
            //{"TWD", "TWD"},
            //{"MMK", "MMK"},
            //{"VND", "VND"},
            //{"PHP", "PHP"},
        };

        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en-US"},
            {"th-TH", "th-TH"},
            {"vi-VN", "vi-VN"},
            {"zh-TW", "zh-TW"},
            {"zh-CN", "zh-CN"},
            {"my-MM", "en-MY"},
            {"ko-KR", "ko-KR"},
            {"ja-JP", "ja-JP"},
            {"id-ID", "id-ID"},
            {"ms-MY", "ms-MY"},
            {"es-ES", "es-ES"},
            {"lo-LAO", "lo-LA"}
        };

        /// <summary>
        /// 注單狀態
        /// 3 當局取消
        /// 4 正常注單
        /// 5 事後取消
        /// 6 改牌
        /// </summary>
        public enum BetStatusEnum
        {
            /// <summary>
            /// 遊戲事前取消
            /// </summary>
            [Description("當局取消")]
            Reject = 3,

            [Description("正常注單")]
            Normal = 4,

            [Description("事後取消")]
            Cancel = 5,

            [Description("改牌")]
            Change = 6,
        }
    }
}
