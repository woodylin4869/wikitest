using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Code
{
    public class MessageCode
    {
        public static Dictionary<int, string> Message = new Dictionary<int, string>()
        {
            {(int)ResponseCode.Success, "Success"},
            //1000 Error for User
            {(int)ResponseCode.LoginFail, "Incorrect user account or password"},
            {(int)ResponseCode.RegisterFail, "Register account fail"},
            {(int)ResponseCode.KickUserFail, "Kick user fail"},
            {(int)ResponseCode.SetLimitFail, "Setting user limit fail"},
            {(int)ResponseCode.SetGroupLimitFail, "Setting group user limit fail"},
            {(int)ResponseCode.GetBalanceFail, "Get user balance fail"},
            {(int)ResponseCode.CreateMemberFail, "Create member fail"},
            {(int)ResponseCode.CreateRcgUserFail, "Create rcg member fail"},
            {(int)ResponseCode.CreateSabaUserFail, "Create saba member fail"},
            {(int)ResponseCode.UpdateMemberFail, "Update member fail"},
            {(int)ResponseCode.CreateWalletFail, "Check or create member wallet fail"},
            {(int)ResponseCode.UserNotFound, "User not found"},
            {(int)ResponseCode.InsufficientBalance, "Insufficient balance"},
            {(int)ResponseCode.InsufficientLockBalance, "Insufficient lock balance"},
            {(int)ResponseCode.CreateRcgUserTokenFail, "Create rcg user token fail"},
            {(int)ResponseCode.GetWMUserFail, "Get WM user fail"},
            {(int)ResponseCode.GetSabaUserFail, "Get saba user fail"},
            {(int)ResponseCode.GetOnlineUserFail, "Get online user fail"},
            {(int)ResponseCode.CreateJdbUserFail, "Create jdb user fail"},
            {(int)ResponseCode.CreateMgUserFail, "Create Mg user fail"},
            {(int)ResponseCode.CreatePgUserFail, "Create Pg user fail"},
            {(int)ResponseCode.CreateDsUserFail, "Create Ds user fail"},
            {(int)ResponseCode.CreateRsgUserFail, "Create Rsg user fail"},
            {(int)ResponseCode.CreateAeUserFail, "Create Ae user fail"},
            {(int)ResponseCode.CreateStremaerUserFail, "Create Rsg user fail"},
            {(int)ResponseCode.CreateTpUserFail, "Create Tp user fail"},
            {(int)ResponseCode.CheckTpUserFail, "Check Tp user fail"},
            {(int)ResponseCode.CheckJokerUserFail, "Check Joker user fail"},
            {(int)ResponseCode.CreateRlgUserTokenFail, "Create Rlg user token fail"},
            {(int)ResponseCode.GetJackpotHistoryFail, "Get jackpot history fail"},
            {(int)ResponseCode.OverStopBalance, "Credit over stop balance"},
            //2000 Error for Permission
            {(int)ResponseCode.InsufficientPermissions, "Insufficient permissions"},
            {(int)ResponseCode.JwtTokenTypeFail, "Jwt token type fail"},
            //3000 Error for Forward Game
            {(int)ResponseCode.GetGameURLFail, "Get game url fail"},
            {(int)ResponseCode.GetGameMaintenanceFail, "Get game maintenance time fail"},
            {(int)ResponseCode.SaveRcgGameTokenFail, "Save rcg game token fail"},
            {(int)ResponseCode.GameApiTimeOut, "Game api time out more than 5 times in 10 mins"},
            {(int)ResponseCode.GameApiMaintain, "Game Maintain"},
            //4000 Error for Transfer
            {(int)ResponseCode.FundTransferFail, "Transfer fund to platform fail"},
            {(int)ResponseCode.FundTransferW1Fail, "Transfer fund to W1 fail"},
            {(int)ResponseCode.FundTransferRcgFail, "Transfer fund to Rcg fail"},
            {(int)ResponseCode.FundTransferSabaFail, "Transfer fund to Saba fail"},
            {(int)ResponseCode.CheckFundTransferFail, "Check transfer fund to platform fail"},
            {(int)ResponseCode.InsertTransferRecordFail, "Insert transfer recordFail"},
            {(int)ResponseCode.UpdateTransferRecordFail, "Update transfer recordFail"},
            {(int)ResponseCode.FundTransferJdbFail, "Transfer fund to Jdb fail"},
            {(int)ResponseCode.FundTransferMgFail, "Transfer fund to Mg fail"},
            {(int)ResponseCode.FundTransferPgFail, "Transfer fund to Pg fail"},
            {(int)ResponseCode.FundTransferDsFail, "Transfer fund to Ds fail"},
            {(int)ResponseCode.FundTransferRsgFail, "Transfer fund to Rsg fail"},
            {(int)ResponseCode.SessionMultiFail, "One more not refund session"},
            {(int)ResponseCode.SessionNotEnd, "User session not end"},
            {(int)ResponseCode.SessionNotFound, "Session not found"},
            {(int)ResponseCode.SessionWithdrawn, "This Session has been withdrawn"},
            {(int)ResponseCode.SessionRefundFail, "Refund amount fail"},
            //5000 Error for Game Record
            {(int)ResponseCode.GetGameRecordFail, "Get game record fail"},
            {(int)ResponseCode.GetSummaryRecordFail, "Get summary record fail"},
            //6000 Error for single wallet
            {(int)ResponseCode.TokenTypeFail, "Token type fail"},
            {(int)ResponseCode.DebitFail, "Debit balance fail"},
            {(int)ResponseCode.CreditFail, "Credit balance fail"},
            {(int)ResponseCode.CancelFail, "Cancel bet fail"},
            {(int)ResponseCode.TransactionTypeFail, "Unknow transaction type"},
            {(int)ResponseCode.WriteTransactionRecordFail, "Write transaction record fail"},
            {(int)ResponseCode.UpdateWalletFail, "Update member wallet fail"},
            //7000 GetGameListFail
            {(int)ResponseCode.GetGameListFail, "Get game list fail"},
            //9000 Error for common
            {(int)ResponseCode.UnknowPlatform, "platform name fail"},
            {(int)ResponseCode.UnknowEenvironment, "Eenvironment name fail"},
            {(int)ResponseCode.UnavailablePlatform, "Unavailable platform"},
            {(int)ResponseCode.UnavailableCurrency, "Unavailable currency"},
            {(int)ResponseCode.Fail, "Fail"},
            {(int)ResponseCode.TimeOut, "Request time out"},
        };
    }

    public enum ResponseCode
    {
        Success = 0,

        //1000 Error for User
        LoginFail = 1001,

        RegisterFail,
        KickUserFail,
        SetLimitFail,
        SetGroupLimitFail,
        GetBalanceFail,
        CreateMemberFail,
        CreateRcgUserFail,
        CreateSabaUserFail,
        UpdateMemberFail,
        CreateWalletFail,
        UserNotFound,
        InsufficientBalance,
        InsufficientLockBalance,
        CreateRcgUserTokenFail,
        GetWMUserFail,
        GetSabaUserFail,
        GetOnlineUserFail,
        CreateJdbUserFail,
        CreateMgUserFail,
        CreatePgUserFail,
        CreateDsUserFail,
        CreateRsgUserFail,
        CreateAeUserFail,
        CreateStremaerUserFail,
        CreateTpUserFail,
        CheckTpUserFail,
        CheckJokerUserFail,
        CreateRlgUserTokenFail,
        GetJackpotHistoryFail,
        OverStopBalance,

        //2000 Error for Permission
        InsufficientPermissions = 2001,

        JwtTokenTypeFail,

        //3000 Error for Forward Game
        GetGameURLFail = 3001,

        SaveRcgGameTokenFail,
        GetGameMaintenanceFail,
        GameApiTimeOut,
        GameApiMaintain,

        //4000 Error for Transfer
        FundTransferFail = 4001,

        FundTransferW1Fail,
        FundTransferSabaFail,
        FundTransferRcgFail,
        FundTransferAeFail,
        CheckFundTransferFail,
        InsertTransferRecordFail,
        UpdateTransferRecordFail,
        FundTransferJdbFail,
        FundTransferMgFail,
        FundTransferPgFail,
        FundTransferDsFail,
        FundTransferRsgFail,
        SessionMultiFail,
        SessionNotEnd,
        SessionNotFound,
        SessionWithdrawn,
        SessionRefundFail,

        //5000 Error for Game Record
        GetGameRecordFail = 5001,

        GetSummaryRecordFail,

        //6000 Error for single wallet
        TokenTypeFail = 6001,

        DebitFail,
        CreditFail,
        CancelFail,
        TransactionTypeFail,
        WriteTransactionRecordFail,
        UpdateWalletFail,

        //7000 Error for Game
        GetGameListFail = 7001,

        //9000 Error for common
        UnknowPlatform = 9001,

        UnknowEenvironment,
        UnavailablePlatform,
        UnavailableCurrency,
        Fail = 9999,
        TimeOut
    }

    public enum Platform
    {
        ALL = 9999,
        H1 = 0,
        W1,
        RCG,
        SABA,
        SBO,
        JDB,
        MG,
        PG,
        DS,
        RSG,
        RLG,
        AE,
        STREAMER, //RCG小娛樂專用
        TP,
        RTG,
        JILI,
        JOKER,
        OB,
        META,
        GR,
        SEXY,
        WM,
        XG,
        NEXTSPIN,
        PP,
        FC,
        WS168,
        SABA2,
        MT,
        PME,
        MP,
        CMD368,
        KS,
        BTI,
        RCG2,
        GEMINI,
        RGRICH,
        EGSLOT,
        CR,
        RCG3,
        WE,
        PS,
        IDN,
        VA,
        STANDARDS,
        SPLUS
    }

    [Flags]
    public enum PlatformType
    {
        None = 0,
        Electronic = 1, //電子遊戲
        Live = 2, //真人
        Sport = 4, //運動
        Chess = 8, //棋牌
        Animal = 16,//動物競技
        ESport = 32, //電子競技
    }

    public enum Currency
    {
        USD = 0, //美元
        THB, //泰銖
        TWD, //TWD
        RMB, //人民幣
        MMK, //緬甸緬元
        VND, //越南盾
        HKD, //港幣
        KRW, //韓圓
        MYR, //馬幣
        SGD, //新加坡元
        JPY,// 日圓
        IDR,// 印尼盾
        EUR,// 歐元
        GBP,// 英鎊
        CHF,// 瑞士法朗
        MXN,// 新墨西哥比索
        CAD,// 加拿大幣
        RUB,// 俄羅斯盧布
        INR,// 印度盧比
        RON,// 羅馬尼亞列伊
        DKK,// 丹麥克朗
        NOK,// 挪威克朗
        PHP,// 菲律賓披索
        LAK,// 寮幣
    }
}