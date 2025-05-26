using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP
{
    public class TP
    {
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en"},
            {"th-TH", "th"},
            {"vi-VN", "vi"},
            {"zh-TW", "zh-cn"},
            {"zh-CN", "zh-cn"},
            {"id-ID", "id"},
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB", "THB"},
        };


        public static class LiveGameMap
        {
            public static readonly Dictionary<string, int> CodeToId = new()
            {
                {"ga_tP", 3289},
                {"ga_sM", 2656},
                {"ga_aH", 2624},
                {"ga_sF", 2372},
                {"ga_a1", 2371},
            };

            public static readonly Dictionary<string, string> IdToCode = new()
            {
                {"3289", "ga_tP"},
                {"2656", "ga_sM"},
                {"2624", "ga_aH"},
                {"2372", "ga_sF"},
                {"2371", "ga_a1"},
                {"1606", "ga_comLobby"},
            };
        }
    }
    public enum error_code
    {
        success = 0,
        Authorization_invalid = 1,
        Sign_invalid = 2,
        Bad_parameters = 3,
        Api_not_found = 4,
        Api_timeout = 5,
        IP_not_allowed_to_access_api = 6,
        Exceed_api_call_limit =7,
        Player_account_duplicated = 101,
        Player_does_not_exist = 102,
        Game_hall_not_exist = 201,
        Game_not_exist = 202,
        Transaction_ID_duplicated = 301,
        Balance_insufficient = 302,
        Transaction_ID_not_exist = 303,
        Search_Time_is_out_of_range = 304,
        Betlog_not_exist = 401,
        Betlog_Bad_parameters = 402,
        Betlog_Permission_Denied = 403,
        Agent_account_duplicated = 501,
        Invalid_allowed_ip_address = 502,
        Game_lobby_service_is_closed =601,
        Kickall_request_duplicate = 701,
        Kickall_Check_key_not_exist = 702,
        Platform_API_config_error = 1000,
        Platform_Bad_parameters = 1001,
        Platform_Sign_invalid = 1002,
        Platform_Api_failed = 1003,
        Platform_Under_maintenance = 1004,
        Platform_Account_duplicated = 1005,
	    Platform_Api_timeout                  =1006,
	    Platform_Transaction_ID_duplicated    =1007,
	    Platform_Insufficient_balance         =1008,
	    Platform_IP_not_allowed_to_access_api =1009,
	    Platform_Exceed_transaction_limit     =1010,
	    Platform_Cannot_withdraw_when_gaming  =1011,
	    Platform_Cannot_kick_player_in_game   =1012,
	    Platform_Account_not_exist            =1013,
	    Platform_Game_not_exist               =1014,
	    Platform_Network_error                =1015,
	    Platform_Switch_Game_Failed           =1016,
	    Platform_Service_not_provided         =1017,
	    Platform_Exceed_create_member_limit   =1018,
	    Platform_Response_data_format_error   =1019,
	    Platform_No_free_link                 =1020,
	    Platform_Transaction_failed           =1021,
        Platform_Invalid_Currency             =1022,
        Platform_Game_under_maintenance       =1023,
        Platform_System_busy                  =1024,
        Platform_Transaction_unknown_error    =1025,
        Platform_Kick_member_failed           =1026,
        Platform_Get_playcheck_failed         =1027,
        Platform_Cannot_deposit_when_gaming   =1032,
        Platform_Something_wrong              =1999
    }
}
