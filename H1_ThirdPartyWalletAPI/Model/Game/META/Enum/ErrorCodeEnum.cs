namespace H1_ThirdPartyWalletAPI.Model.Game.META.Enum
{
    public enum ErrorCodeEnum
    {
        Success = 200,
        Failure = 1001,
        Internal_Error = 1002,
        Insufficient = 1003,
        Agent_Error = 2008,
        Setting_Not_Completed = 2011,
        Member_Not_Found = 2104,
        Account_Already_Exists = 2105,
        Account_Has_Been_Frozen = 2106,
        Account_Cannot_Bet = 2107,
        Account_Disabled = 2108,
        Point_Transfer_Failed = 2110,
        Failed_To_Add_Deduction_Points = 2111,
        Insufficient_Member_Balance = 2114,
        Failed_To_Add_New_Member = 2116,
        Game_Table_Is_Not_Open_Yet = 2117,
        Agent_Market_Does_Not_Exist = 2232,
        Member_Market_Does_Not_Exist = 2233,
        Account_Length_Error = 2234,
        Password_Length_Error = 2235,
        Language_Error = 2236,
        Duplicate_Tracking_Number = 2237,
        Game_Not_Found = 3001,
        Bet_Slip_Does_Not_Exist = 3109,
        HashKey_Or_HashIv_Error = 7004
    }
}