using System.Collections.Generic;
using System.Collections.Immutable;

namespace H1_ThirdPartyWalletAPI.Model.Game.PME
{
    public static class PME
    {
        public static readonly ImmutableDictionary<string, string> Lang = new Dictionary<string, string>()
        {
            {"en-US", "en"},  // 英文
            {"zh-TW", "zh"},  // 繁體中文
            {"zh-CN", "cn"},  // 簡體中文
            {"th-TH", "th"},  // 泰文
            {"ko-KR", "ko"},  // 韓文
            {"vi-VN", "vi"},  // 越南文
            {"ja-JP ", "jp"},  // 日文
            {"id-ID", "ni"},  // 印尼文
        }.ToImmutableDictionary();


        public static readonly ImmutableDictionary<string, int> Currency = new Dictionary<string, int>()
        {
            {"THB", 26},  // 泰铢
        }.ToImmutableDictionary();


        public static readonly ImmutableDictionary<long, int> GameCode = new Dictionary<long, int>()
        {
            {257154660915053, 1}, //英雄联盟
            {257289795134339, 2}, //DOTA2
            {257366452854033, 3}, //任天堂全明星大乱斗
            {257393813865855, 4}, //彩虹6号
            {257421187099429, 5}, //使命召唤
            {257445934728386, 6}, //堡垒之夜
            {257474469784520, 7}, //绝地求生
            {257532676759026, 8}, //守望先锋
            {257561197207055, 9}, //王者荣耀
            {257578064923863, 10}, //CSGO
            {257594184113856, 11}, //星际争霸
            {257620516983156, 12}, //街头霸王
            {257647180647306, 13}, //万智牌
            {257672679460102, 14}, //雷神之锤
            {257697372071315, 15}, //篮球
            {257719680296134, 16}, //足球
            {257733146228414, 17}, //NBA2K
            {258003077224510, 18}, //魔兽争霸III
            {258006510157846, 19}, //星际争霸2
            {258143683216896, 20}, //神之浩劫
            {258172821136137, 21}, //火箭联盟
            {258336811299100, 22}, //魔兽世界
            {258337013509741, 23}, //FIFA
            {271192272576750, 24}, //无畏契约
            {614598069861073, 25}, //炉石传说
            {1377370776735090, 26}, //穿越火线
            {21363218739142692, 27}, //激门峡谷
            {31476623018398892, 28}, //和平精英
            {1376613872342718, 29}, //传说对决
            {1377323217127607, 30}, //皇室战争
            {101258749512819384, 31}, //无尽对决
            {159003446163958838, 32}, //无人机冠军联盟
            {1001, 33}, //虚拟足球
            {1002, 34}, //虚拟赛狗
            {1004, 35}, //虚拟篮球
            {1010, 36}, //虚拟摩托车
            {1011, 37}, //虚拟赛马
            {10158428472914228, 38} //英雄召唤
        }.ToImmutableDictionary();
    }

    public enum TransferType
    {
        Deposit = 1,
        Withdraw = 2
    }
}
