using System.Collections.Generic;
using System.Collections.Immutable;

namespace H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN
{
    public static class NEXTSPIN
    {
        public static readonly ImmutableDictionary<string, string> Lang = new Dictionary<string, string>()
        {
            {"en-US", "en_US"},
            {"th-TH", "th_TH"},
            {"vi-VN", "vi_VN"},
            {"zh-TW", "zh_CN"},
            {"zh-CN", "zh_CN"},
            {"id-ID", "id_ID"},
        }.ToImmutableDictionary();

        public static readonly ImmutableDictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB", "THB"},
        }.ToImmutableDictionary();

        public enum ErrorCode
        {
            Success = 0,
            System_Error = 1,
            Invalid_Request = 2,
            Service_Inaccessible = 3,
            Request_Timeout = 100,
            Call_Limited = 101,
            Request_Forbidden = 104,
            Missing_Parameters = 105,
            Invalid_Parameters = 106,
            Duplicated_Serial_No = 107,
            Merchant_Key_Error = 108,
            Record_Id_Not_Found = 110,
            Api_Call_Limited = 112,
            Invalid_Acct_Id = 113,
            //Acct_Not_Found = 10103,
            Password_Invalid = 10104,
            //Acct_Inactive = 10105,
            //Acct_Locked = 10110,
            //Acct_Suspend = 10111,
            Merchant_Not_Found = 10113,
            Bet_Insufficient_Balance = 11101,
            Bet_Draw_Stop_Bet = 11102,
            Bet_Type_Not_Open = 11103,
            Bet_Info_Incomplete = 11104,
            Bet_Acct_Info_Incomplete = 11105,
            Bet_Request_Invalid = 11108,
            //Bet_Setting_Incomplete = 12001,
            Bet_Setting_Incomplete = 30003,
            Acct_Not_Found = 50100,
            Acct_Inactive = 50101,
            Acct_Locked = 50102,
            Acct_Suspend = 50103,
            Token_Validation_Failed = 50104,
            Insufficient_Balance = 50110,
            Exceed_Max_Amount = 50111,
            Currency_Invalid = 50112,
            Amount_Invalid = 50113,
            Game_Currency_Not_Active = 50200,
            Web_Server_Is_Down = 521,
            Bet_Request_Invalid_Max = 1110801,
            Bet_Request_Invalid_Min = 1110802,
            Bet_Request_Invalid_Totalbet = 1110803,
        }
    }
}
