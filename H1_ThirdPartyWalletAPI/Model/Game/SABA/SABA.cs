using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game
{
    public class SABA
    {
        public static Dictionary<string, int> Currency = new Dictionary<string, int>()
        {
            {"USD", 3},
            {"THB", 4},
            {"UUS", 20},
        };
        public static Dictionary<string, string> BetFrom = new Dictionary<string, string>()
        {
            {"Asia", "d"},
            {"M_Text", "m"},
            {"M_H5", "l"},
            {"China", "z"},
            {"New_Asia", "x"},
            {"M_Lite", "c"},
            {"Quick", "t"},
        };
        public static Dictionary<string, int> Odds_Type = new Dictionary<string, int>()
        {
            {"Special", 0},
            {"Malay_Odds", 1},
            {"China_Odds", 2},
            {"Decimal_Odds", 3},
            {"Indo_Odds", 4},
            {"American_Odds", 5},
        };
        public static Dictionary<string, int> Device = new Dictionary<string, int>()
        {
            {"DESKTOP", 1},
            {"MOBILE", 2},
            {"MOBILE_TEXT", 3},
        };
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "&lang=en"},
            {"th-TH", "&lang=th"},
            {"vi-VN", "&lang=vn"},
            {"zh-TW", "&lang=ch"},
            {"zh-CN", "&lang=cs"},
        };
    }
    //SABA all request model
    public class SABA_CreateMember
    {
        public string vendor_id { get; set; }
        public string vendor_member_id { get; set; }
        public string operatorid { get; set; }
        public string username { get; set; }
        public string oddstype { get; set; }
        public int currency { get; set; }
        public decimal maxtransfer { get; set; }
        public decimal mintransfer { get; set; }
    }
    public class SABA_UpdateMember
    {
        public string vendor_id { get; set; }
        public string vendor_member_id { get; set; }
        public string oddstype { get; set; }
        public decimal maxtransfer { get; set; }
        public decimal mintransfer { get; set; }
    }
    public class SABA_KickUser
    {
        public string vendor_id { get; set; }
        public string vendor_member_id { get; set; }
    }
    public class SABA_CheckIsOnline
    {
        public string vendor_id { get; set; }
        public string vendor_member_ids { get; set; }
    }
    public class SABA_CheckUserBalance
    {
        public string vendor_id { get; set; }
        public string vendor_member_ids { get; set; }
        public int wallet_id { get; set; }
    }
    public class SABA_FundTransfer
    {
        public string vendor_id { get; set; }
        public string vendor_member_id { get; set; }
        public string vendor_trans_id { get; set; }
        public decimal amount { get; set; }
        public int currency { get; set; }
        public int direction { get; set; }
        public int wallet_id { get; set; }
    }
    public class SABA_CheckFundTransfer
    {
        public string vendor_id { get; set; }
        public string vendor_trans_id { get; set; }
        public int wallet_id { get; set; }
    }
    public class SABA_GetBetDetail
    {
        public string vendor_id { get; set; }
        public Int64 version_key { get; set; }
    }
    public class SABA_SetMemberBetSetting
    {
        public string vendor_id { get; set; }
        public string vendor_member_id { get; set; }
        public string bet_setting { get; set; }
    }
    public class SABA_BetSetting
    {
        public string sport_type { get; set; }
        public decimal min_bet { get; set; }
        public decimal max_bet { get; set; }
        public decimal max_bet_per_match { get; set; }
        public decimal max_payout_per_match { get; set; }
        public SABA_BetSetting()
        {
            max_payout_per_match = 5000000;
        }
    }

    public class SABA_SetMemberBetSettingBySubsidiary
    {
        public string vendor_id { get; set; }
        public string operatorId { get; set; }
        public int currency { get; set; }
        public string bet_setting { get; set; }
    }
    public class SABA_GetSabaUrl
    {
        public string vendor_id { get; set; }
        public string vendor_member_id { get; set; }
        public int platform { get; set; }
    }
    public class SABA_GetBetDetailByTimeframe
    {
        public string vendor_id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public int time_type { get; set; }
    }
    public class SABA_GetBetDetailByTransID
    {
        public string vendor_id { get; set; }
        public long trans_id { get; set; }
        public int bet_type { get; set; }
    }
    public class SABA_GetMaintenanceTime
    {
        public string vendor_id { get; set; }
    }
    public class SABA_GetOnlineUserCount
    {
        public string vendor_id { get; set; }
    }
    public class SABA_GetFinancialReport
    {
        public string vendor_id { get; set; }
        public string financial_date { get; set; }
        public int currency { get; set; }
        public enum ErrorCode
        {
            Success = 0,
            Failed,
            MerchantTokenFail = 9,
            SystemMaintain = 10
        }
    }

    public class SABA_GetBetSettingLimit
    {
        public string vendor_id { get; set; }
        public int currency { get; set; }
    }

    //SABA all response model
    public class SABA_ResBase
    {
        public int error_code { get; set; }
        public string message { get; set; }
    }
    public class SABA_CreateMember_Res : SABA_ResBase
    {
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            UserNameFail,
            OperatorIdFail,
            OddsTypeFail,
            CurrencyFail,
            VendorMemberIdFail,
            MinTransferOverMax,
            PrefixFail,
            MerchantTokenFail,
            SystemMaintain,
            LengthOver30 = 12,
            MaxtransferOverLimit
        }
    }
    public class SABA_UpdateMember_Res : SABA_ResBase
    {
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            UserNameFail,
            MerchantTokenFail = 9,
            SystemMaintain,
        }
    }
    public class SABA_KickUser_Res : SABA_ResBase
    {
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            UserNameFail,
            UserOffline,
            MerchantTokenFail = 9,
            SystemMaintain,
        }
    }
    public class SABA_CheckIsOnline_Res : SABA_ResBase
    {
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            UserNameFail,
            MerchantTokenFail = 9,
            SystemMaintain,
        }
    }
    public class SABA_FundTransfer_Res : SABA_ResBase
    {
        public SABA_FundTransfer_Res_Data Data { get; set; }
        public enum Status
        {
            OK = 0,
            Failed,
            Pending
        }
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            UserNameFail,
            InsufficientBalance,
            LimitFail,
            Duplicate_Tid,
            CurrencyFail,
            ParameterFail,
            PlayerWinLimit,
            MerchantTokenFail,
            SystemMaintain,
            SystemBusy,
            PrefixFail,
            MemberUnlockFail,
            MemeberLcoked = 15,
            OneWalletFail,
        }
    }
    public class SABA_CheckFundTransfer_Res : SABA_ResBase
    {
        public SABA_CheckFundTransfer_Res_Data Data { get; set; }
        public enum Status
        {
            OK = 0,
            Failed,
            Pending
        }
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            TidNotExist,
            Pending,
            WalletIdFail = 7,
            MerchantTokenFail = 9,
            SystemMaintain,
        }
    }
    public class SABA_CheckUserBalance_Res : SABA_ResBase
    {
        public int status { get; set; }
        public List<SABA_MemberBalanceData> Data { get; set; }
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            UserNameFail,
            GetOtherBalanceFail = 7,
            MerchantTokenFail = 9,
            SystemMaintain,
        }
    }
    public class SABA_GetSabaUrl_Res : SABA_ResBase
    {
        public string Data { get; set; }
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            UserNameFail,
            ParameterFail,
            MerchantTokenFail = 9,
            SystemMaintain,
        }
    }
    public class SABA_GetBetDetail_Res : SABA_ResBase
    {
        public SABA_Game_Record Data { get; set; }
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            MerchantTokenFail = 9,
            SystemMaintain
        }
    }
    public class SABA_GetBetDetailByTimeframe_Res : SABA_ResBase
    {
        public SABA_Game_Record Data { get; set; }
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            StartLessEnd,
            MerchantTokenFail = 9,
            SystemMaintain
        }
    }
    public class SABA_GetBetDetailByTransID_Res : SABA_ResBase
    {
        public object Data { get; set; }
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            MerchantTokenFail = 9,
            SystemMaintain
        }
    }
    public class SABA_GetMaintenanceTime_Res : SABA_ResBase
    {
        public SABA_MaintenanceTimeData Data { get; set; }
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            Nofund,
            Noconnection,
            SystemMaintain = 10
        }
    }
    public class SABA_SetMemberBetSetting_Res : SABA_ResBase
    {
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            UserNameFail,
            Fail,
            SportTypeFail,
            ParameterFail = 7,
            MerchantTokenFail = 9,
            SystemMaintain,
        }
    }
    public class SABA_SetMemberBetSettingBySubsidiary_Res : SABA_ResBase
    {
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            UserNameFail,
            Fail,
            SportTypeFail,
            OperatorIdFail,
            ParameterFail = 7,
            MerchantTokenFail = 9,
            SystemMaintain,
        }
    }
    public class SABA_GetOnlineUserCount_Res : SABA_ResBase
    {
        public List<SABA_GetOnlineUserCountData> Data { get; set; }
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            NoMerchant,
            MerchantTokenFail = 9,
            SystemMaintain = 10
        }
    }
    public class SABA_GetFinancialReport_Res : SABA_ResBase
    {
        public SABA_GetFinancialReportData Data { get; set; }

    }

    public class SABA_GetBetSettingLimit_Res : SABA_ResBase
    {
        public SABA_GetBetSettingLimit_Data[] Data { get; set; }
        public enum ErrorCode
        {
            Success = 0,
            SystemError,
            CurrencyFail,
            MerchantTokenFail = 9,
            SystemMaintain = 10
        }
    }

    //SABA response data model
    public class SABA_MemberBalanceData
    {
        public string vendor_member_id { get; set; }
        public decimal? balance { get; set; }
        public decimal? bonus_balance { get; set; }
        public decimal? outstanding { get; set; }
        public int currency { get; set; }
        public int error_code { get; set; }
    }
    public class SABA_FundTransfer_Res_Data
    {
        public long trans_id { get; set; }
        public decimal before_amount { get; set; }
        public decimal after_amount { get; set; }
        public decimal bonus_before_amount { get; set; }
        public decimal bonus_after_amount { get; set; }
        public string system_id { get; set; }
        public int status { get; set; }
    }
    public class SABA_CheckFundTransfer_Res_Data
    {
        public long trans_id { get; set; }
        public DateTime? transfer_date { get; set; }
        public decimal amount { get; set; }
        public int currency { get; set; }
        public decimal before_amount { get; set; }
        public decimal after_amount { get; set; }
        public int status { get; set; }
    }
    public class SABA_Game_Record
    {
        public Int64 last_version_key { get; set; }
        public List<SABA_BetDetails> BetDetails { get; set; }
        public List<SABA_BetDetails> BetNumberDetails { get; set; } = new();
        public List<SABA_BetDetails> BetVirtualSportDetails { get; set; } = new();
        public SABA_Game_Record()
        {
            BetDetails = new List<SABA_BetDetails>();
        }
    }
    public class SABA_GetBetDetailByTransID_Data
    {
        public Int64 last_version_key { get; set; }
        public List<SABA_BetDetails> BetDetails { get; set; }
    }

    public class SABA_BetDetails
    {
        private Int64? default_parlay_ref_no = 0;

        public Guid summary_id { get; set; }
        public Int64 last_version_key { get; set; }
        public Int64? trans_id { get; set; } //交易單號
        public string vendor_member_id { get; set; } //會員id
        public string operator_id { get; set; } //公司id
        public int? league_id { get; set; } //聯賽id
        public int? match_id { get; set; } //賽事id
        public int? sport_type { get; set; } //體育類型
        public int? bet_type { get; set; } //投注類型
        public string bet_type_en { get; set; } //投注類型名稱
        public Int64? parlay_ref_no { get => default_parlay_ref_no; set => default_parlay_ref_no = (value != null ? value : 0); } //混合過關注單號碼
        public decimal? odds { get; set; } //賠率
        public decimal? stake { get; set; } //投注
        public decimal? pre_stake { get; set; } //原始投注
        public DateTime? transaction_time { get; set; } //投注交易時間
        public DateTime? winlost_datetime { get; set; } //結算時間 (對帳使用)
        public DateTime? settlement_time { get; set; } //注單結算的時間,僅支援 sport_type:1~99
        public string ticket_status { get; set; } //注單狀態
        public decimal? winlost_amount { get; set; } //輸或贏的金額
        public decimal? pre_winlost_amount { get; set; } //原始輸或贏的金額
        public decimal? after_amount { get; set; } //下注後的餘額
        public int? currency { get; set; } //幣別
        public int? odds_type { get; set; } //賠率盤口
        public int? islive { get; set; } //是否在滾球下注 1:是 0:否 
        public int? balancechange { get; set; } //餘額是否更動 1:是 0:否 
        public decimal? winlost { get; set; } //前次結算輸或贏的金額
        public List<SABA_SportInfo> leaguename { get; set; } //聯賽名稱
        public List<SABA_SportInfo> hometeamname { get; set; } //主隊名稱
        public List<SABA_SportInfo> awayteamname { get; set; } //客隊名稱
        public string bet_team { get; set; } //投注隊伍
        public List<SABA_ResettlementInfo> resettlementinfo { get; set; } //重新结算信息(若有重新结算才会出现)
        //public string leaguename_en => (leaguename == null)? null: leaguename.FirstOrDefault(x => x.lang == "en").name;
        //public string hometeamname_en => (hometeamname == null) ? null : hometeamname.FirstOrDefault(x => x.lang == "en").name;
        //public string awayteamname_en => (awayteamname == null) ? null : awayteamname.FirstOrDefault(x => x.lang == "en").name;
        public string leaguename_en { get; set; }
        public string hometeamname_en { get; set; }
        public string awayteamname_en { get; set; }
        public string club_id { get; set; }
        public string franchiser_id { get; set; }

        public decimal? turnover { get; set; } //有效投注
        public DateTime report_time { get; set; } //報表時間
        public DateTime partition_time { get; set; } //分區時間




        public List<object> ParlayData { get; set; }

    }
    public class SABA_ResettlementInfo
    {
        public DateTime ActionDate { get; set; } //重新結算時間
        public bool balancechange { get; set; } //余额是否更动.(false 或 true)
        public decimal? winlost { get; set; } //前次结算输或赢的金额
    }
    public class SABA_SportInfo
    {
        public string lang { get; set; }
        public string name { get; set; }
    }
    public class SABA_MaintenanceTimeData
    {
        public bool IsUM { get; set; }
        public DateTime? UMStartDateTime { get; set; }
        public DateTime? UMEndDateTime { get; set; }
    }
    public class SABA_GetOnlineUserCountData
    {
        public string platform { get; set; }
        public int count { get; set; }
    }
    public class SABA_GetFinancialReportData
    {
        public string Merchant { get; set; }
        public string FinancialDate { get; set; }
        public string Currency { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal TotalWinAmount { get; set; }
        public int TotalBetCount { get; set; }
        public decimal NetAmount { get; set; }
        public int MemberCount { get; set; }
        public int CancelticketCount { get; set; }
        public int CancelRoundCount { get; set; }
        public decimal TotalTurnover { get; set; }
    }

    public class SABA_GetBetSettingLimit_Data
    {
        public string sport_type { get; set; }

        public decimal min_bet { get; set; }

        public decimal max_bet { get; set; }

        public decimal max_bet_per_match { get; set; }

        public decimal? max_payout_per_match { get; set; }
    }

}
