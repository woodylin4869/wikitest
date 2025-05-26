using System.Collections.Generic;
using System.ComponentModel;

namespace ThirdPartyWallet.Share.Model.Game.RCG3
{
    public class RCG3
    {
        public enum msgId
        {
            Success = 0, //成功
            Fail = -1, //其它失敗或錢包有餘額，不可更新幣別
            PasswardFail = -2, //密碼錯誤
            MemberOnline = -4, //會員在線
            Suspend = -5, //停用
            MemberLocked = -6, //會員鎖定
            MemberBanned = -7, //會員凍結
            MemberNotFound = -8, //無此帳號
            GameClosed = -9, //遊戲關閉
            SystemMaintain = -10, //系統維護
            CreditZero = -11, //額度為0
            TimeOut = -12,
        }

        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"RMB", "CNY"},
            {"HKD", "HKD"},
            {"KRW", "KRW"},
            {"MYR", "MYR"},
            {"SGD", "SGD"},
            {"USD", "USD"},
            {"JPY", "JPY"},
            {"THB", "THB"},
            {"IDR", "IDR"},
            {"EUR", "EUR"},
            {"GBP", "GBP"},
            {"CHF", "CHF"},
            {"MXN", "MXN"},
            {"CAD", "CAD"},
            {"RUB", "RUB"},
            {"INR", "INR"},
            {"RON", "RON"},
            {"DKK", "DKK"},
            {"NOK", "NOK"},
            {"TWD", "TWD"},
            {"MMK", "MMK"},
            {"VND", "VND"},
            {"PHP", "PHP"},
            {"LAK", "LAK"},
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

        public static class LiveGameMap
        {
            public static readonly Dictionary<string, int> CodeToId = new()
            {
                // {桌別代碼, w1自訂編號} // 館別/遊戲類型/桌號/桌別名稱
                {"1913130003", 1001}, // GClub/牛牛/8101/牛牛R-A
                {"2007230101", 1002}, // GClub/三寶百家樂/9101/三寶百家樂R-A
                {"2012160121", 1003}, // GClub/區塊鏈百家樂/9401/區塊鏈百家樂B-A
                {"2012160122", 1004}, // GClub/區塊鏈百家樂/9402/區塊鏈百家樂B-B
                {"2012160123", 1005}, // GClub/區塊鏈百家樂/9403/區塊鏈百家樂B-C
                {"2012160124", 1006}, // GClub/區塊鏈百家樂/9404/區塊鏈百家樂B-D
                {"2012160125", 1007}, // GClub/區塊鏈百家樂/9405/區塊鏈百家樂B-E
                {"2104220110", 1008}, // GClub/區塊鏈百家樂/9406/區塊鏈百家樂B-F
                {"2104220111", 1009}, // GClub/區塊鏈百家樂/9407/區塊鏈百家樂B-G
                {"2104220112", 1010}, // GClub/區塊鏈百家樂/9408/區塊鏈百家樂B-H
                {"2104220113", 1011}, // GClub/區塊鏈百家樂/9409/區塊鏈百家樂B-I
                {"2104220114", 1012}, // GClub/區塊鏈百家樂/9410/區塊鏈百家樂B-J
                {"2105140113", 1013}, // GClub/百家樂/1112/現場百家樂L-A
                {"2105140114", 1014}, // GClub/百家樂/1113/現場百家樂L-B
                {"2109071301", 1015}, // GClub/區塊鏈龍虎/9501/區塊鏈龍虎B-A
                {"2109141302", 1016}, // GClub/區塊鏈龍虎/9502/區塊鏈龍虎B-B
                {"2110131303", 1017}, // GClub/區塊鏈龍虎/9503/區塊鏈龍虎B-C
                {"2110131304", 1018}, // GClub/區塊鏈龍虎/9504/區塊鏈龍虎B-D
                {"2110131305", 1019}, // GClub/區塊鏈龍虎/9505/區塊鏈龍虎B-E
                {"2110210702", 1020}, // GClub/博丁/1701/博丁R-A
                {"2111171501", 1021}, // GClub/安達巴哈/15101/安達巴哈R-A
                {"2112071601", 1022}, // GClub/色碟/16101/色碟R-A
                {"2201050139", 1023}, // GClub/百家樂/4113/百家樂P-A
                {"2201050140", 1024}, // GClub/百家樂/4114/百家樂P-B
                {"2201050141", 1025}, // GClub/百家樂/4115/極速百家樂P-C
                {"2201050142", 1026}, // GClub/百家樂/4116/百家樂P-D
                {"2201050143", 1027}, // GClub/百家樂/4117/極速百家樂P-E
                {"2201050144", 1028}, // GClub/百家樂/4118/百家樂P-F
                {"2201111701", 1029}, // GClub/泰國骰/17001/泰國骰R-A
                {"2203171401", 1030}, // GClub/區塊鏈射龍門/18001/區塊鏈射龍門B-A
                {"GCBC20201101", 1031}, // GClub/百家樂/1101/極速百家樂C-A
                {"GCBC20201102", 1032}, // GClub/百家樂/1102/極速百家樂C-B
                {"GCBC20201103", 1033}, // GClub/百家樂/1103/百家樂C-C
                {"GCBC20201104", 1034}, // GClub/百家樂/1104/百家樂C-D
                {"GCBC20201105", 1035}, // GClub/百家樂/1105/免傭百家樂C-E
                {"GCBC20201106", 1036}, // GClub/百家樂/1106/免傭百家樂C-F
                {"GCBC20201107", 1037}, // GClub/百家樂/1107/極速百家樂R-A
                {"GCBC20201108", 1038}, // GClub/百家樂/1108/極速百家樂R-B
                {"GCBC20201109", 1039}, // GClub/百家樂/1109/極速百家樂R-C
                {"GCBC20201110", 1040}, // GClub/百家樂/1110/極速百家樂R-D
                {"GCBC20201111", 1041}, // GClub/百家樂/1111/G-K百家樂
                {"GCFT20201501", 1042}, // GClub/番攤/1501/骰子番攤R-A
                {"GCIB20201601", 1043}, // GClub/保險百家樂/1601/保險百家樂R-A
                {"GCLH20201201", 1044}, // GClub/龍虎/1201/極速龍虎R-A
                {"GCLH20201202", 1045}, // GClub/龍虎/1202/極速龍虎R-C
                {"GCLH20201203", 1046}, // GClub/龍虎/1203/G-C龍虎
                {"GCLP20201301", 1047}, // GClub/輪盤/1301/輪盤R-A
                {"GCLP20201302", 1048}, // GClub/輪盤/1302/輪盤R-B
                {"GCSZ20201401", 1049}, // GClub/骰寶/1401/骰寶R-A
                {"GCSZ20201402", 1050}, // GClub/骰寶/1402/骰寶R-B
                {"2308080503", 1051}, // GClub/番攤/0503/番攤 R-B
                {"2308080504", 1052}, // GClub/番攤/0504/番攤 R-C
                {"MSBC20203101", 2001}, // MStar/百家樂/3101/免傭百家樂C-G
                {"MSBC20203102", 2002}, // MStar/百家樂/3102/百家樂C-H
                {"MSBC20203103", 2003}, // MStar/百家樂/3103/百家樂C-I
                {"MSBC20203104", 2004}, // MStar/百家樂/3104/極速百家樂R-E
                {"MSBC20203105", 2005}, // MStar/百家樂/3105/極速百家樂R-F
                {"MSBC20203106", 2006}, // MStar/百家樂/3106/極速百家樂R-G
                {"MSBC20203107", 2007}, // MStar/百家樂/3107/極速百家樂R-H
                {"MSBC20203108", 2008}, // MStar/百家樂/3108/極速百家樂R-I
                {"MSIB20203601", 2009}, // MStar/保險百家樂/3601/保險百家樂R-B
                {"MSLH20203201", 2010}, // MStar/龍虎/3201/龍虎R-B
                {"MSLH20203202", 2011}, // MStar/龍虎/3202/極速龍虎R-D
                {"MSLP20203301", 2012}, // MStar/輪盤/3301/輪盤R-C
                {"MSLP20203302", 2013}, // MStar/輪盤/3302/極速輪盤R-D
                {"MSSZ20203401", 2014}, // MStar/骰寶/3401/極速骰寶R-C
                {"MBC20202101", 3001}, // MClub/百家樂/2101/百家樂M-A
                {"MBC20202102", 3002}, // MClub/百家樂/2102/百家樂M-B
                {"MBC20202103", 3003}, // MClub/百家樂/2103/百家樂M-C
                {"MBC20202104", 3004}, // MClub/百家樂/2104/百家樂M-D
                {"MLH20202201", 3005}, // MClub/龍虎/2201/龍虎M-A
                {"MLH20202202", 3006}, // MClub/龍虎/2202/龍虎M-B
                {"MLH20202203", 3007}, // MClub/龍虎/2203/極速龍虎M-C
                {"MLP20202301", 3008}, // MClub/輪盤/2301/輪盤M-A
                {"MPD20202701", 3009}, // MClub/博丁/2701/博丁M-A
                {"MSZ20202401", 3010}, // MClub/骰寶/2401/骰寶M-A
                {"SYBC20204101", 3011}, // MClub/百家樂/4101/百家樂Sexy-A
                {"SYBC20204102", 3012}, // MClub/百家樂/4102/百家樂Sexy-B
                {"SYBC20204103", 3013}, // MClub/百家樂/4103/百家樂Sexy-C
                {"SYBC20204104", 3014}, // MClub/百家樂/4104/百家樂Sexy-D
                {"SYLH20204201", 3015}, // MClub/龍虎/4201/龍虎Sexy-A
                {"SYLH20204202", 3016}, // MClub/龍虎/4202/龍虎Sexy-B
                {"2111240131", 4001}, // XG/百家樂/4107/百家C-J
                {"2111240132", 4002}, // XG/百家樂/4108/百家C-K
                {"2111240133", 4003}, // XG/百家樂/4109/百家C-L
                {"2111240134", 4004}, // XG/百家樂/4110/百家C-M
                {"2111240135", 4005}, // XG/百家樂/4111/百家C-N
                {"2111240136", 4006}, // XG/百家樂/4112/百家C-O
                {"2112160137", 4007}, // XG/百家樂/4105/百家C-Y
                {"2112160138", 4008}, // XG/百家樂/4106/百家C-Z
                {"2212290211", 4009}, // XG/龍虎/2001/龍虎C-A
                {"2212290306", 4010}, // XG/骰寶/4001/骰寶C-A
                {"2212290406", 4011}, // XG/輪盤/3001/輪盤C-A
                {"2205030146", 5001}, // WM/百家樂/4302/百家C-U
                {"2205030147", 5002}, // WM/百家樂/4303/百家C-V
                {"2205030148", 5003}, // WM/百家樂/4304/百家C-W
                {"2205030149", 5004}, // WM/百家樂/4305/百家C-X
                {"2209130150", 5005}, // WM/百家樂/4306/百家W-A
                {"2209130151", 5006}, // WM/百家樂/4307/百家W-B
                {"2209130152", 5007}, // WM/百家樂/4308/百家W-C
                {"2209130153", 5008}, // WM/百家樂/4309/百家W-D
                {"2212290201", 5009}, // WM/龍虎/2002/龍虎W-A
                {"2212290301", 5010}, // WM/骰寶/4002/骰寶W-A
                {"2212290401", 5011}, // WM/輪盤/3002/輪盤W-A
                {"2212290501", 5012}, // WM/番攤/5001/番攤W-A
            };
        }
        public class RCG_Login
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
            public int welletMode { get; set; }
            public string gameDeskID { get; set; }
            //public int itemNo { get; set; }
            public string backUrl { get; set; }
            //public int gameDeskID { get; set; }
            //public int itemNo { get; set; }
            public string lang { get; set; }
            public string groupLimitID { get; set; }
        }
        public class RCG_CreateOrSetUser
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
            public string memberName { get; set; }
            public int stopBalance { get; set; }
            public string betLimitGroup { get; set; }
            public string currency { get; set; }
            public string language { get; set; }
            public string openGameList { get; set; }

            public string h1SHIDString { get; set; }
            public RCG_CreateOrSetUser()
            {
                stopBalance = -1;
                betLimitGroup = "1,2,3";
                openGameList = "ALL";

            }
        }
        public class RCG_KickOut
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
        }
        public class RCG_KickOutByCompany
        { }
        public class RCG_GetBetLimit
        { }
        public class RCG_GetBalance
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
        }
        public class RCG_GetPlayerOnlineList
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
        }
        public class RCG_Deposit
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
            public string transactionId { get; set; }
            public decimal transctionAmount { get; set; }
        }
        public class RCG_Withdraw
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
            public string transactionId { get; set; }
            public decimal transctionAmount { get; set; }
        }
        public class RCG_GetBetRecordList
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public long maxId { get; set; }
            public long rows { get; set; }
        }
        public class RCG_GetGameDeskList
        { }
        public class RCG_GetTransactionLog
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string transactionId { get; set; }
        }
        public class RCG_GetMaintenanceInfo
        { }

        public class RCG_ResBase<T>
        {
            public int msgId { get; set; }
            public string message { get; set; }
            public int timestamp { get; set; }
            public T data { get; set; }
        }
        public class RCG_Login_Res
        {
            public string url { get; set; }
            public string token { get; set; }
        }
        public class RCG_CreateOrSetUser_Res
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
        }
        public class RCG_KickOut_Res
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
        }
        public class RCG_KickOutByCompany_Res
        {
            public int msgId { get; set; }
            public string message { get; set; }
            public int timestamp { get; set; }
            public bool data { get; set; }
        }
        public class RCG_GetBetLimit_Res
        { }
        public class RCG_GetBalance_Res
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
            public decimal balance { get; set; }
            public string online { get; set; }
            public string memberType { get; set; }
        }
        public class RCG_GetPlayerOnlineList_Res
        {
            public List<PlayerData> dataList { get; set; }
        }
        public class PlayerData
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
            public string serverNo { get; set; }
            public string ip { get; set; }
            public string device { get; set; }
            public string loginTime { get; set; }
        }
        public class RCG_Deposit_Res
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
            public decimal transactionAmount { get; set; }
            public decimal balance { get; set; }
            public string transactionId { get; set; }
            public long transactionTime { get; set; }
        }
        public class RCG_Withdraw_Res
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
            public decimal transactionAmount { get; set; }
            public decimal balance { get; set; }
            public string transactionId { get; set; }
            public long transactionTime { get; set; }
        }
        //public class RCG_GetBetRecordList_Res
        //{
        //    public int msgId { get; set; }
        //    public string message { get; set; }
        //    public int timestamp { get; set; }
        //    public GetBetRecordListData data { get; set; }
        //}
        public class RCG_GetBetRecordList_Res
        {
            public string systemCode { get; set; }
            public string webId { get; set; }
            public List<BetRecord> dataList { get; set; }
        }
        public class BetRecord
        {
            public Guid summary_id { get; set; }
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
            public long id { get; set; }
            public int gameId { get; set; }
            public string desk { get; set; }
            public string betArea { get; set; }
            public decimal bet { get; set; }
            public decimal available { get; set; }
            public decimal winLose { get; set; }
            public decimal waterRate { get; set; }
            public string activeNo { get; set; }
            public string runNo { get; set; }
            public decimal balance { get; set; }
            public DateTime dateTime { get; set; }
            public DateTime reportDT { get; set; }
            public string ip { get; set; }
            public decimal odds { get; set; }
            public long? originRecordId { get; set; }
            /// <summary>
            /// 原始下注金額
            /// </summary>
            public decimal? pre_bet { get; set; }
            /// <summary>
            /// 原始有效下注
            /// </summary>
            public decimal? pre_available { get; set; }
            /// <summary>
            /// 原始輸贏
            /// </summary>
            public decimal? pre_winlose { get; set; }
            /// <summary>
            /// 原始注單編號
            /// </summary>
            public long? pre_id { get; set; }
        }
        public class RCG_GetGameDeskList_Res
        { }

        public class RCG_GetTransactionLog_Res
        {
            public string transactionId { get; set; }
            public string systemCode { get; set; }
            public string webId { get; set; }
            public string memberAccount { get; set; }
            public int transactionType { get; set; }
            public decimal transactionAmount { get; set; }
            public decimal beforeBalance { get; set; }
            public decimal afterBalance { get; set; }
            public int status { get; set; }
            public DateTime transactionTime { get; set; }
        }
        public class TransactionLog
        {

        }
        public class RCG_GetMaintenanceInfo_Res
        {
            public string Name { get; set; }
            public int MaintainType { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

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
