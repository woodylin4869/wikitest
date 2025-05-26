using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Enum
{
    public enum ErrorCodeEnum
    {
        [Description("正常")]
        OK = 0,

        [Description("失敗")]
        ExecuteFailed = 100,

        [Description("［token］參數異常")]
        TokenParameterException = 200,

        [Description("［data］參數異常")]
        dataParameterException =  201,

        [Description("［key］參數異常")]
        KeyParameterException =  202,

        [Description("［merID］參數異常")]
        MerIDParameterException =  203,

        [Description("查無 ［ParentName］ 資料")]
        ParentNameParameterException =  204,

        [Description("［Account］參數異常")]
        AccountParameterException =  205,

        [Description("［Balance］ 參數異常")]
        BalanceParameterException =  206,

        [Description("［Balance］ 需為正整數")]
        BalanceIsPositiveNumber =  207,

        [Description("［PageIndex］ 不能小於等於 0")]
        PageIndexNotLessThanzero =  208,

        [Description("［PageSize］ 不能小於等於 0")]
        PageSizeNotLessThanzero =  209,

        [Description("［PageSize］ 不能大於 100")]
        PageSizeNoGreaterThanOnehundred =  210,

        [Description("［StartTime］ 驗證錯誤")]
        StartTimeError =  211,

        [Description("［EndTime］ 驗證錯誤")]
        EndTimeError =  212,

        [Description("［OrderNumber］ 參數異常")]
        OrderNumberParameterException =  213,

        [Description("［Kickback］ 參數異常")]
        KickbackParameterException =  214,

        [Description("［Merchantkey］ 參數異常")]
        MerchantkeyParameterException =  215,

        [Description("［ProvidersKey］ 參數異常")]
        ProvidersKeyParameterException =  216,

        [Description("［betid］ 參數異常")]
        betidParameterException =  217,

        [Description("［Currency］ 參數異常")]
        CurrencyParameterException =  218,

        [Description("［X-API-ClientID］ 參數異常")]
        XAPIClientIDParameterException =  219,

        [Description("［X-API-Signature］ 參數異常")]
        XAPISignatureParameterException =  220,

        [Description("［X-API-Timestamp］ 參數異常")]
        XAPITimestampParameterException =  221,

        [Description("［Msg］ 參數異常")]
        MsgParameterException =  222,

        [Description("［SiteUrl］ 參數異常")]
        SiteUrlParameterException =  224,

        [Description("［X-API-ClientID］ 驗證錯誤")]
        XAPIClientIDError =  225,


        [Description("［X-API-Signature］ 驗證錯誤")]
        XAPISignatureError =  226,

        [Description("［X-API-Timestamp］ 驗證錯誤")]
        XAPITimestampError =  227,

        [Description("［Msg］ 驗證錯誤")]
        MsgError =  228,

        [Description("［SetOption］ 參數異常")]
        SetOptionParameterException =  229,

        [Description("［SetOp］ 參數異常")]
        SetOpParameterException =  230,

        [Description("參數異常")]
        ParameterException =  231,

        [Description("驗證錯誤")]
        Error =  232,

        [Description("資料解析異常")]
        DataParameterException =  233,

        [Description("結束時間與起始時間不能相差超過 24 小時")]
        TimeDifference24 =  234,

        [Description("［SetOption］ 參數異常")]
        SetoptionParameterException =  235,

        [Description("［Msg］資料解密失敗")]
        MsgDecryptError =  301,

        [Description("［Msg］資料解析異常")]
        MsgAbnormal =  302,

        [Description("商家號與參數資料不一致")]
        ParametersAreInconsistent =  303,

        [Description("商家資料類型錯誤")]
        TypeError =  304,

        [Description("［MemberID］ 資料解析異常")]
        MemberIDParameterException =  305,

        [Description("查無商家號資料")]
        CheckNoBusiness =  306,

        [Description("查無商家號對應的會員資料")]
        NoMemberInformation =  307,

        [Description("查無 ［WebId］ 對應的會員資料")]
        WebIdNoMember =  308,

        [Description("商家號 與 ［WebId］ 無法對應")]
        WebIdDifferent =  309,

        [Description("［Level］ 資料解析異常")]
        LevelParameterException =  310,

        [Description("查無系統代碼資料")]
        SystemCodeError =  312,

        [Description("［SystemCode］ 資料解析異常")]
        SystemCodeParameterException =  313,

        [Description("［WebId］ 資料解析異常")]
        WebIdParameterException =  314,

        [Description("沒有該彩別 ID，或是該彩別並無開放")]
        NoIotteryId =  400,

        [Description("查無彩別資料")]
        NoInquireIotter =  402,

        [Description("查無期數資料")]
        NoPeriodData =  402,

        [Description("指定的彩別不提供這個設定")]
        InquireNoSetup =  403,

        [Description("指定的彩別不存在設定資料")]
        InquireNoData =  404,

        [Description("目前無期數")]
        NoPeriod =  405,

        [Description("期數資料異常")]
        PeriodDataError =  406,

        [Description("後飛號碼重複設定")]
        DuplicateCodeNumber =  407,

        [Description("玩法不存在")]
        GameplayNotExist =  408,

        [Description("開盤中，設定金額不能低於下注實量")]
        AmountNotLessThan =  409,

        [Description("玩法資料錯誤")]
        PlayerDataError =  411,

        [Description("玩法資料錯誤［Ａ］")]
        PlayerDataErrorA =  412,

        [Description("玩法資料錯誤［B］")]
        PlayerDataErrorB =  413,

        [Description("玩法資料錯誤［C］")]
        PlayerDataErrorC =  413,

        [Description("玩法資料錯誤［D］")]
        PlayerDataErrorD =  413,

        [Description("查無彩別群組資料")]
        CheckNoGroupData =  416,

        [Description("會員建立失敗")]
        buildfailed =  500,

        [Description("會員資料驗證失敗")]
        memberdatacheckfail =  501,

        [Description("組織類型錯誤")]
        OrganizeTypeError =  502,

        [Description("上級資料錯誤")]
        UpperLayerDataError =  503,

        [Description("會員資料修改失敗")]
        UPmemberdataError =  504,

        [Description("系統層級錯誤")]
        systemlevelError =  505,

        [Description("代理層級錯誤")]
        AgencylevelError =  506,

        [Description("會員層級錯誤")]
        memberleveError =  507,

        [Description("金額格式錯誤")]
        moneyformatError =  600,

        [Description("訂單編號產生失敗！")]
        Ordernumbergenerationfailed =  601,

        [Description("訂單編號寫入失敗！")]
        OrderwriteError =  602,

        [Description("帳單產生失敗！")]
        Billgenerationfailed =  603,

        [Description("充值失敗！")]
        Rechargefailed =  604,

        [Description("提款失敗！")]
        Withdrawalfailed =  605,

        [Description("查無此帳單")]
        Checkthisbill =  607,

        [Description("代理額度不足")]
        InsufficientAgentQuota =  608,

        [Description("會員額度不足")]
        InsufficientMembershipQuota =  609,

        [Description("查無注單資料")]
        InquireNotbetdata =  701,

        [Description("注單資料異常")]
        betdataError =  702,

        [Description("資料已存在")]
        DataExist = 900211,
        TransactionIsNotFound = 900212,
    }
}
