using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.AE
{
    public class AE
    {
        /// <summary>
        /// 語系 Mapping
        /// key: W1, Value: 遊戲商
        /// </summary>
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "enUS"},  // 英文
            {"zh-TW", "zhTW"},  // 繁體中文
            {"zh-CN", "zhCN"},  // 簡體中文
            {"th-TH", "thTH"},  // 泰文
            {"ko-KR", "koKR"},  // 韓文
            {"vi-VN", "viVN"},  // 越南文

            // W1 語系
            // 英文 en-US
            // 泰文 th-TH
            // 越南文 vi-VN
            // 繁中 zh-TW
            // 簡中 zh-CN
            // 緬文 en-MY
            // 韓文 ko-KR


            // AE 语言码 Language Code
            // enUS
            // zhTW
            // zhCN
            // jaJP
            // koKR
            // thTH
            // viVN
            // esES
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖

            // W1 貨幣
            // 美金 USD
            // 泰銖 THB
            // 台幣 TWD
            // 人民幣 RMB
            // 緬元 MMK
            // 韓圓 KRW
            // 越南盾 VND


            // AE 货币码 Currency Code
            // CNY
            // HKD
            // JPY
            // KRW
            // THB
            // MYR
            // EUR
            // GBP
            // USD
            // IDR
            // IDR_1000
            // VND
            // VND_1000
            // TWD
            // SGD
            // INR
            // PHP
            // MMK
            // MMK_1000
            // NZD
            // MNT
            // AUD
            // BND
            // BDT
            // COP
            // PEN
            // LAK
            // KHR
            // MXN
            // USDT
        };
    }
}