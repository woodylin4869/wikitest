namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Enum
{
    public enum ErrorCodeEnum
    {
        Success = 0,
        Failure = 1000,
        Exception = 1002 ,
        Insufficient = 1003 ,
        ILLEGAL_ARGUMENT = 1005 ,
        Illegal_arguments = 1006 ,
        Type_Error = 1007 ,
        CreateUserError=1101 ,
        UpdateUserError=1102 ,
        Useringame=1103 ,
        Userpermissionsdisabled=1104,
        User_permissions_are_abnormal=1105,
        User_does_not_exist = 1106 ,
        Transaction_Identification_Unique_Code_Repeat = 1107 ,
        Turn_point_failed = 1108 ,
        No_such_transaction = 1109 ,
        Currency_code_error = 1201 ,
        Language_code_error = 1202 ,
        Failed_to_create_GameToken = 1203 ,
        Game_code_error = 1301 ,
        The_game_is_under_maintenance = 1302 ,
        Game_all_kick_error = 1303 ,
        Player_is_offline = 1304 ,
        Player_match_table = 1305 ,
        Player_settlement = 1306 ,
        Record_is_not_exist = 1401 ,
	}
}