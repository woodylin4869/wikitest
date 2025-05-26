using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG
{
    public class RTG
    {
        /// <summary>
        /// 語系 Mapping
        /// key: W1, Value: 遊戲商
        /// </summary>
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en-US"},  // 英文
            {"zh-TW", "zh-TW"},  // 繁體中文
            {"zh-CN", "zh-CN"},  // 簡體中文
            {"th-TH", "th-TH"},  // 泰文
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖
        };
        public static Dictionary<int, string> GameList = new Dictionary<int, string>()
        {
            {1001,"Pok Pead Pok Gao"},
            {1002,"Gao Gae"},
            {1003,"Chinese Poker"},
            {1004,"Pai Gow Poker"},
            {1005,"Texas Holdem"},
            {1006,"Mahjong"},
            {1007,"Royal Horse Racing"},
            {1008,"Fightbull"},
            {1009,"Golden flowers"},
            {1010,"Ultimate Monopoly"},
            {1011,"Hundreds PokDeng"},
            {1012,"Royal Dog Racing"},
            {1013,"Hundreds golden flower"},
            {1014,"Hundreds Niu Niu"},
            {1015,"The battle dice"},
            {1016,"Last Card"},
            {1017,"Crazy Snake"},
            {1018,"Gao Gae 4 cards"},
            {1019,"Thai-SicBo"},
            {1020,"PhaiKhang"},
            {1021,"Gao Gae-Billionaire"},
            {1022,"Fish Prawn Crab"},
            {1023,"6+Holdem"},
            {1024,"PokDeng"},
        };
    }
}