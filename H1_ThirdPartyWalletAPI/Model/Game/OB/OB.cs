
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.OB
{
    public class OB
    {

        public static Dictionary<string, int> oddType = new Dictionary<string, int>()
        {
            {"A", 4445},
            {"B", 4446},
            {"C", 4447},
            {"D", 4438},
            {"E", 4439},
            {"F", 4440},
            {"G", 4441},
            {"H", 4442},
            {"I", 4443},
            {"J", 4444},
        };

        public static Dictionary<string, string> VersionEnum = new Dictionary<string, string>()
        {
            {"V1", "1"},
            {"V2", "2"},
        };

        public static Dictionary<string, int> lang = new Dictionary<string, int>()
        {
            {"zh-CN", 1},  // 簡體中文
            {"zh-TW", 2},  // 繁體中文
            {"en-US", 3},  // 英文
            {"ja-JP", 4},  // 日文
            {"ko-KR", 5},  // 英文            
            {"th-TH", 6},  // 泰文
            {"vi-VN", 7},  // 越南文
            {"id-ID", 8},  // 印尼文
        };

        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB","THB"},  // 泰銖
        };

        public static Dictionary<string, string> Bacc = new Dictionary<string, string>()
        {
            {"庄", "Banker"},  // 莊家
            {"闲","Player"},  // 閒
            {"和","Tie"},  // 和局
            {"庄对","Banker Pair"},
            {"闲对","Player Pair"},
            {"龙虎和"  ,"Dragon Tiger Tie"},
            {"老虎和"  ,"Tiger Tie"},
            {"老虎对"  ,"Tiger Pair"},
            {"完美对子","Perfect Pair"},
            {"庄龙宝"  ,"Banker Dragon Bonus"},
            {"闲龙宝"  ,"Player Dragon Bonus"},
            {"超级六"  ,"Super Six"},
            {"庄免佣"  ,"Banker No Commission"},
            {"任意对子","Either Pair"},
            {"超和(0)"    ,"Super Tie(0)"},
            {"超和(1)"    ,"Super Tie(1)"},
            {"超和(2)"    ,"Super Tie(2)"},
            {"超和(3)"    ,"Super Tie(3)"},
            {"超和(4)"    ,"Super Tie(4)"},
            {"超和(5)"    ,"Super Tie(5)"},
            {"超和(6)"    ,"Super Tie(6)"},
            {"超和(7)"    ,"Super Tie(7)"},
            {"超和(8)"    ,"Super Tie(8)"},
            {"超和(9)"    ,"Super Tie(9)"},
            {"超级对"  ,"Super Pair"},
            {"龙7"    ,"Dragon7"},
            {"熊猫8"   ,"Panda8"},
            {"大老虎"  ,"Big Tiger"},
            {"小老虎"  ,"Small Tiger"},
            {"庄天牌"  ,"Banker Natural"},
            {"闲天牌"  ,"Player Natural"},
            {"天牌"    ,"Natural"},
            {"龙"     ,"Dragon"},
            {"虎"     ,"Tiger"},
        };

        public static Dictionary<string, string> LongHu = new Dictionary<string, string>()
        {
            {"龙", "Dragon"},  // 龍
            {"虎", "Tiger"},  // 虎
            {"和","Tie"},  // 和局
        };

        public static Dictionary<string, string> LunPan = new Dictionary<string, string>()
        {
            {"单"      , "Odd"},
            {"双"      , "Even"},
            {"大"      ,"Big"},
            {"小"      ,"Small"},
            {"红"      ,"Red"},
            {"黑"      ,"Black"},
            {"第一打"  ,"Dozen1"},
            {"第二打"  ,"Dozen2"},
            {"第三打"  ,"Dozen3"},
            {"打一"    ,"Dozen1"},
            {"打二"    ,"Dozen2"},
            {"打三"    ,"Dozen3"},
            {"直注"    ,"Direct"},
            {"分注"    ,"Split"},
            {"街注"    ,"Street"},
            {"三数"    ,"3Digit"},
            {"四个号码","4Number"},
            {"角注"    ,"Corner"},
            {"线注"    ,"Line"},
            {"列一"    ,"Column1"},
            {"列二"    ,"Column2"},
            {"列三"    ,"Column3"},

        };


        public static Dictionary<string, string> Blackjack = new Dictionary<string, string>()
        {
            {"庄"      ,   "Banker"},
            {"闲"      ,   "Player"},
            {"21+3"    ,   "21+3"},
            {"完美对子" ,   "Perfect Pair"},
            {"保险"    ,   "Insurance"},
            {"旁注"    ,   "Side"},
            {"底注"    ,   "Ante"},
            {"分牌"    ,   "Split"},
            {"加倍"    ,   "Multiply"},
        };


        public static Dictionary<string, string> PaiGowPok = new Dictionary<string, string>()
        {
            {"顺门"     ,   "Clockwise"},
            {"出门"     ,   "Out"},
            {"到门"     ,   "Anticlockwise"},
            {"庄门"     ,   "Banker"},
            {"顺门赢"   ,   "Clockwise Win"},
            {"顺门输"   ,   "Clockwise Lose"},
            {"出门赢"   ,   "Out Win"},
            {"出门输"   ,   "Out Lose"},
            {"到门赢"   ,   "Anticlockwise Win"},
            {"到门输"   ,   "Anticlockwise Lose"},
        };


        public static Dictionary<string, string> ShaiZi = new Dictionary<string, string>()
        {
            {"和值"   ,   "Sum"},
            {"大"     ,   "Big"},
            {"小"     ,   "Small"},
            {"单"     ,   "Odd"},
            {"双"     ,   "Even"},
            {"围骰"   ,   "Round Dice"},
            {"全围"   ,   "Triple"},
            {"单点"   ,   "Single Point"},
            {"对子"   ,   "Pair"},
            {"牌九式" ,   "Pai Gow"},
        };

        public static Dictionary<string, string> ThreeTrumps = new Dictionary<string, string>()
        {
            {"庄"          ,   "B"},
            {"闲"          ,   "P"},
            {"赢"          ,   "win"},
            {"输"          ,   "lose"},
            {"单公"        ,   "one Face"},
            {"双公"        ,   "Two Face"},
            {"三公"        ,   "Three Face"},
            {"闲1对牌以上"  , "P1Pair Plus"},
            {"闲2对牌以上"  ,   "P2Pair Plus"},
            {"闲3对牌以上"  ,   "P3Pair Plus"},
            {"和","Tie" }
        };

        public static Dictionary<string, string> Niuniu = new Dictionary<string, string>()
        {
            {"庄"          ,   "B "},
            {"闲"          ,   "P "},
            {"平倍"        ,   " Equal"},
            {"翻倍"        ,   " Double"},
            {"牛"          ,   "Bull"},
            {"无"          ,   "No "},
            {"一"          ,   "1 "},
            {"二"          ,   "2 "},
            {"三"          ,   "3 "},
            {"四"          ,   "4 "},
            {"五"          ,   "5 "},
            {"六"          ,   "6 "},
            {"七"          ,   "7 "},
            {"八"          ,   "8 "},
            {"九"          ,   "9 "},
        };

        public static Dictionary<string, string> FanTan = new Dictionary<string, string>()
        {
            {"和值"   ,   "Sum"},
            {"单"     ,   "Odd"},
            {"双"     ,   "Even"},
            {"番"     ,   "Fan"},
            {"念"     ,   "Nim"},
            {"角"     ,   "Kwok"},
            {"四通"   ,   " 4Nga"},
            {"三通"   ,   " 3Nga"},
            {"二通"   ,   " 3Nga"},
            {"一通"   ,   " 1Nga"},
            {"三门"   ,   "SsH"},
        };

        public static Dictionary<string, string> Texas = new Dictionary<string, string>()
        {
            {"庄"       ,   "B "},
            {"闲"       ,   "P "},
            {"底注"     ,   "Ante"},
            {"跟注"     ,   "Call"},
            {"边注"     ,   " Side"},
        };

        public static Dictionary<string, string> AndarBahar = new Dictionary<string, string>()
        {
            {"安达"       ,   "Andar"},
            {"巴哈"       ,   "Bahar"},
        };
        public static Dictionary<string, string> SeDie = new Dictionary<string, string>()
        {
            {"大"     ,   "Big"},
            {"小"     ,   "Small"},
            {"单"     ,   "Odd"},
            {"双"     ,   "Even"},
        };
        public static Dictionary<string, string> Winthreecards = new Dictionary<string, string>()
        {
           {"龙"      ,  "Dragon"},
           {"凤"      ,  "Phoenix"},
           {"对子"    ,  "Pair"},
           {"散牌"    ,  "High card"},
           {"豹子"    ,  "Three of a Kind"},
           {"同花顺"  ,  "Straight Flush"},
           {"同花"    ,  "Flush"},
           {"顺子"    ,  "Straight"},
           {"对8以上" ,  "Pair 8 Plus"},
        };

        public static Dictionary<string, string> TeenPatti = new Dictionary<string, string>()
        {
           {"A"     ,  "A" },
           {"B"     ,  "B" },
           {"和"     ,  "Tie" },
           {"A对+"  ,  "A pair+"},
           {"B对+"  ,  "B pair+"},
           {"红利六",  "SBonus 6"},
        };


        public static Dictionary<string, string> CardType = new Dictionary<string, string>()
        {
            {"0" ,  "♣1" },
            {"1" ,  "♦1" },
            {"2" ,  "♥1" },
            {"3" ,  "♠1" },
            {"4" ,  "♣2" },
            {"5" ,  "♦2" },
            {"6" ,  "♥2" },
            {"7" ,  "♠2" },
            {"8" ,  "♣3" },
            {"9" ,  "♦3" },
            {"10" , "♥3" },
            {"11" , "♠3" },
            {"12" , "♣4" },
            {"13" , "♦4" },
            {"14" , "♥4" },
            {"15" , "♠4" },
            {"16" , "♣5" },
            {"17" , "♦5" },
            {"18" , "♥5" },
            {"19" , "♠5" },
            {"20" , "♣6" },
            {"21" , "♦6" },
            {"22" , "♥6" },
            {"23" , "♠6" },
            {"24" , "♣7" },
            {"25" , "♦7" },
            {"26" , "♥7" },
            {"27" , "♠7" },
            {"28" , "♣8" },
            {"29" , "♦8" },
            {"30" , "♥8" },
            {"31" , "♠8" },
            {"32" , "♣9" },
            {"33" , "♦9" },
            {"34" , "♥9" },
            {"35" , "♠9" },
            {"36" , "♣10" },
            {"37" , "♦10" },
            {"38" , "♥10" },
            {"39" , "♠10" },
            {"40" , "♣J" },
            {"41" , "♦J" },
            {"42" , "♥J" },
            {"43" , "♠J" },
            {"44" , "♣Q" },
            {"45" , "♦Q" },
            {"46" , "♥Q" },
            {"47" , "♠Q" },
            {"48" , "♣K" },
            {"49" , "♦K" },
            {"50" , "♥K" },
            {"51" , "♠K" },
        };
    }
}
