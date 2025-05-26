using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Model.OneWalletGame
{
    public class OW_SABA
    {
        enum status
        {
            Undefine = -1,
            Success = 0,
            DuplicateTransaction = 1,
            ParameterError = 101,
            UserClosed = 201,
            UserLocked = 202,
            NoUser = 203,
            UserStop = 204,
            UserExist = 205,
            NoOperatorID = 301,
            CurrencyFail = 302,
            UserIdFail = 303,
            LanguageFail = 304,
            TokenFail = 305,
            TimeZoneFail = 306,
            AccountFail = 307,
            TimeFormatFail = 308,
            TransactionTypeFail = 309,
            BetLimitFail = 310,
            SecurityKeyFail = 311,
            IPAddressFail =312,
            PermissonFail = 501,
            InsufficientBalance = 502,
            ForbiddenIPAdress = 503,
            BetRecordNotFound = 504,
            NoData = 505,
            ExcuteFailTryAgain = 506,
            BetRecordBonusFail = 507,
            DataBaseFail = 901,
            NetWorkFail = 902,
            SystemMaintain = 903,
            RequestTimeOut = 904,
            SystemBusy = 905,
            SystemError = 999
        }
    }
    public class RequestBaseMessage<T>
    {
        public int key { get; set; }
        public T msg { get; set; }
}
    public class ResponseBaseMessage
    {
        public int status { get; set; }
        public string msg { get; set; }
    }
    public class RequestGetBalance
    {
        public string action { get; set; }
        public string userId { get; set; }
    }
    public class ResponseGetBalance : ResponseBaseMessage
    {
        public string userId { get; set; }
        public string balance { get; set; }
        public string balanceTs { get; set; }
    }
    public class RequestPlaceBet
    {
        public string action { get; set; } //PlaceBet
        public string operationId { get; set; }// 交易纪录 id
        public string userId { get; set; } 
        public int currency { get; set; }
        public int matchId { get; set; } //賽事id 例如：3562795
        public int homeId { get; set; }
        public int awayId { get; set; }
        public string homeName { get; set; }
        public string awayName { get; set; }
        public string kickOffTime { get; set; } // 赛事开始时间 (yyyy-MM-dd HH:mm:ss.SSS) GMT-4
        public string betTime { get; set; } //下注时间 (yyyy-MM-dd HH:mm:ss.SSS) GMT-4
        public decimal betAmount { get; set; } //注单金额
        public decimal actualAmount { get; set; } //实际注单金额
        public int sportType { get; set; }
        public string sportTypeName { get; set; } //例如：Soccer
        public int betType { get; set; }
        public int betTypeName { get; set; } //例如：Handicap
        public short oddsType { get; set; } //例如：1, 2, 3, 4, 5
        public int oddsId { get; set; }
        public decimal odds { get; set; } //例如：-0.95, 0.7
        public string betChoice{ get; set; } 
        public string betChoice_en { get; set; }// betChoice 的英文语系名称。例如：Over, 4-3
        public string updateTime { get; set; }//更新时间 (yyyy-MM-dd HH:mm:ss.SSS) GMT-4
        public int leagueId { get; set; }//聯賽id
        public string leagueName { get; set; }
        public string leagueName_en { get; set; }
        public string sportTypeName_en { get; set; } //体育类型的英文语系名称。 e.g. Soccer
        public string betTypeName_en { get; set; } //投注类型的英文语系名称。 e.g. Handicap
        public string homeName_en { get; set; } //主队名称的英文语系名称。e.g. Chile (V)
        public string awayName_en { get; set; } //客队名称的英文语系名称。e.g. France (V)
        public string IP { get; set; } //
        public bool isLive { get; set; } //滾球?
        public string refId { get; set; } //唯一id
        public string tsId { get; set; } //选填，用户登入会话 id，由厂商提供
        public string point { get; set; } //球头在百练赛中(sporttype= 161)表示下注时，前一颗的球号
        public string point2 { get; set; } //(string) 球头 2 适用于 bettype = 646 才会有值, point = HDP, point2 = OU
        public string betTeam { get; set; } //下注对象
        public int? homeScore { get; set; } //下注时主队得分。在百练赛中(sporttype= 161)表示已开出大于 37.5 的球数
        public int? awayScore { get; set; } //下注时客队得分。在百练赛中(sporttype= 161)表示已开出小于 37.5 的球数, e.g.1
        public bool baStatus { get; set; } //会员是否为 BA 状态。 false:是 / true:否
        public string excluding { get; set; } //当 bet_team=aos 时,才返回此字段,返回的值代表会员投注的正确比分不为列出的这些
        public string betFrom { get; set; } //下注平台
        public decimal creditAmount { get; set; } //需增加在玩家的金额
        public decimal debitAmount { get; set; } //需从玩家扣除的金额
        public string oddsInfo { get; set; } //适用于 bettype = 468,469 才会有值。
        public string matchDatetime { get; set; } //开赛时间 (yyyy-MM-dd HH:mm:ss.SSS) GMT-4 提示: Outright Betting 的 matchDatetime 會是 KickOffTime.
    }
    public class ResponsePlaceBet : ResponseBaseMessage
    {
        public string refId { get; set; }
        public string licenseeTxId { get; set; }
    }

    public class RequestConfirmBet 
    {
        public string action { get; set; } //ConfirmBet
        public string operationId { get; set; }// 交易纪录 id
        public string userId { get; set; }
        public string updateTime { get; set; }
        public string transactionTime { get; set; }
        public List<ConfirmBetTicketInfo> txns { get; set; }
    }
    public class ConfirmBetTicketInfo
    {
        public long txId { get; set; } //沙巴体育系统交易 id
        public string licenseeTxId { get; set; }// 厂商系统交易 id
        public decimal odds { get; set; }
        public short oddsType { get; set; }
        public decimal actualAmount { get; set; }
        public bool isOddsChanged { get; set; }
        public decimal creditAmount { get; set; }
        public decimal debitAmount { get; set; }
        public string winlostDate { get; set; }
    }
    public class ResponseConfirmBet : ResponseBaseMessage
    {
        public decimal balance { get; set; }
    }

    public class RequestCancelBet 
    {
        public string action { get; set; } //CancelBet
        public string operationId { get; set; }// 交易纪录 id
        public string userId { get; set; }
        public string updateTime { get; set; }
        public List<CancelTicketInfo> txns { get; set; }
    }
    public class CancelTicketInfo
    {
        public string refId { get; set; } //唯一 id.
        public decimal creditAmount { get; set; }
        public decimal debitAmount { get; set; }
    }
    public class ResponseCancelBet : ResponseBaseMessage
    {
        public decimal balance { get; set; }
    }

    public class RequestSettle 
    {
        public string action { get; set; } //Settle 
        public string operationId { get; set; }// 交易纪录 id
        public List<SettleTicketInfo> txns { get; set; }
    }
    public class SettleTicketInfo
    {
        public string userId { get; set; } //用户 id.
        public string refId { get; set; } //唯一 id.
        public long txId { get; set; } //沙巴体育系统交易 id
        public string updateTime { get; set; }
        public string winlostDate { get; set; }
        public string status { get; set; }//(string) 交易结果 half won/half/lose/won/lose/draw/void/refund/reject
        public decimal payout { get; set; } //注单赢回的金额
        public decimal creditAmount { get; set; }
        public decimal debitAmount { get; set; }
        public List<ParlayDetail> parlayDetail { get; set; }
    }

    public class ParlayDetail
    {
        public long refNo { get; set; } //parlay_ref_no from GetBetDetail
        public decimal stake { get; set; } //注单金额
        public decimal odds { get; set; }
        public decimal transOdds { get; set; }
        public string ticketStatus { get; set; } //交易结果 half won/half/lose/won/lose/draw/void/refund/reject
        public string winlostDate { get; set; } //决胜时间(仅显示日期) (yyyy-MM-dd 00:00:00.000) GMT-4
        public decimal winlostAmount { get; set; } //注单结算的金额
        public List<SystemParlayDetail> systemParlayDetail { get; set; }
    }
    public class SystemParlayDetail
    {
        public long sn { get; set; } // 唯一 id.
        public string detailWinlostDate { get; set; } //决胜时间(仅显示日期) (yyyy-MM-dd 00:00:00.000) GMT-4
        public string ticketStatus { get; set; } //交易结果 half won/half/lose/won/lose/draw/void/refund/reject
        public int leagueId { get; set; }//聯賽id
        public string leagueName_en { get; set; }
        public string leagueName_cs { get; set; }
        public int matchId { get; set; } //賽事id 例如：3562795
        public int homeId { get; set; }
        public string homeName_en { get; set; }
        public string homeName_cs { get; set; }
        public int awayId { get; set; }
        public string awayName_en { get; set; }
        public string awayName_cs { get; set; }
        public string matchDatetime { get; set; } //开赛时间 (yyyy-MM-dd HH:mm:ss.SSS) GMT-4 提示: Outright Betting 的 matchDatetime 會是 KickOffTime.
        public decimal odds { get; set; } //例如：-0.95, 0.7
        public int betType { get; set; }
        public string betTypeName_en { get; set; } //例如：Over/Under
        public string betTypeName_cs { get; set; } //例如：大小盘
        public string betTeam { get; set; } //下注对象
        public int sportType { get; set; }
        public string sportTypeName_en { get; set; } //例如：Soccer
        public string sportTypeName_cs { get; set; } //例如：足球
        public bool isLive { get; set; } //滾球?
        public int? homeScore { get; set; } //下注时主队得分。在百练赛中(sporttype= 161)表示已开出大于 37.5 的球数
        public int? awayScore { get; set; } //下注时客队得分。在百练赛中(sporttype= 161)表示已开出小于 37.5 的球数, e.g.1        
    }

    public class ResponseSettle : ResponseBaseMessage {}

    public class RequestReSettle 
    {
        public string action { get; set; } //Resettle 
        public string operationId { get; set; }// 交易纪录 id
        public List<SettleTicketInfo> txns { get; set; }
    }
    public class ResponseReSettle : ResponseBaseMessage { }

    public class RequestUnSettle 
    {
        public string action { get; set; } //Unsettle 
        public string operationId { get; set; }// 交易纪录 id
        public List<SettleTicketInfo> txns { get; set; }
    }
    public class ResponseUnSettle : ResponseBaseMessage { }

    public class RequestPlaceBetParlay
    {
        public string action { get; set; } //PlaceBetParlay 
        public string operationId { get; set; }// 交易纪录 id
        public string userid { get; set; }
        public int currency { get; set; }
        public string betTime { get; set; }
        public string updateTime { get; set; }
        public decimal totalBetAmount { get; set; }
        public string IP { get; set; }
        public string tsId { get; set; }
        public string betFrom { get; set; }
        public decimal creditAmount { get; set; }
        public decimal debitAmount { get; set; }
        public List<ComboInfo> txns { get; set; }
        public List<TicketDetail> ticketDetail { get; set; }
    }
    public class TicketDetail
    {
        public int matchId { get; set; } //賽事id 例如：3562795
        public int homeId { get; set; }
        public int awayId { get; set; }
        public string homeName { get; set; }
        public string awayName { get; set; }
        public string kickOffTime { get; set; } // 赛事开始时间 (yyyy-MM-dd HH:mm:ss.SSS) GMT-4
        public int sportType { get; set; }
        public string sportTypeName { get; set; } //例如：Soccer
        public int betType { get; set; }
        public int betTypeName { get; set; } //例如：Handicap
        public int oddsId { get; set; }
        public decimal odds { get; set; } //例如：-0.95, 0.7
        public short oddsType { get; set; } //例如：1, 2, 3, 4, 5
        public string betChoice { get; set; }
        public string betChoice_en { get; set; }// betChoice 的英文语系名称。例如：Over, 4-3
        public int leagueId { get; set; }//聯賽id
        public string leagueName { get; set; }
        public bool isLive { get; set; } //滾球?
        public string point { get; set; } //球头在百练赛中(sporttype= 161)表示下注时，前一颗的球号
        public string point2 { get; set; } //(string) 球头 2 适用于 bettype = 646 才会有值, point = HDP, point2 = OU
        public string betTeam { get; set; } //下注对象
        public int? homeScore { get; set; } //下注时主队得分。在百练赛中(sporttype= 161)表示已开出大于 37.5 的球数
        public int? awayScore { get; set; } //下注时客队得分。在百练赛中(sporttype= 161)表示已开出小于 37.5 的球数, e.g.1
        public bool baStatus { get; set; } //会员是否为 BA 状态。 false:是 / true:否
        public string excluding { get; set; } //当 bet_team=aos 时,才返回此字段,返回的值代表会员投注的正确比分不为列出的这些
        public string leagueName_en { get; set; }
        public string sportTypeName_en { get; set; } //体育类型的英文语系名称。 e.g. Soccer
        public string homeName_en { get; set; } //主队名称的英文语系名称。e.g. Chile (V)
        public string awayName_en { get; set; } //客队名称的英文语系名称。e.g. France (V)
        public string betTypeName_en { get; set; } //投注类型的英文语系名称。 e.g. Handicap       
        public string matchDatetime { get; set; } //开赛时间 (yyyy-MM-dd HH:mm:ss.SSS) GMT-4 提示: Outright Betting 的 matchDatetime 會是 KickOffTime.
    }
    public class ComboInfo
    {
        public string refid { get; set; }//唯一 id
        public string parlayType { get; set; } //例如：Parlay_Mix, Parlay_System, Parlay_Lucky, SingleBet_ViaLucky
        public decimal betAmount { get; set; }
        public decimal creditAmount { get; set; }
        public decimal debitAmount { get; set; }
        //public List<string> detail { get; set; } //detail先不實作
    }

    public class ResponsePlaceBetParlay
    {
        public List<TicketInfoMapping> txns { get; set; }
    }
    public class TicketInfoMapping
    {
        public string refId { get; set; }
        public string licenseeTxId { get; set; }
    }
    public class RequestConfirmBetParlay
    {
    }
    public class ResponseConfirmBetParlay
    {
    }

    public class RequestPlaceBet3rd
    {
    }
    public class ResponsePlaceBet3rd
    {
    }

    public class RequestConfirmBet3rd
    {
    }
    public class ResponseConfirmBet3rd
    {
    }

    public class RequestCashOut
    {
    }
    public class ResponseCashOut
    {
    }

    public class RequestCashOutReSettle
    {
    }
    public class ResponseCashOutReSettle
    {
    }

    public class RequestUpdateBet
    {
    }
    public class ResponseUpdateBet
    {
    }

    public class RequestHealthCheck
    {
    }
    public class ResponseHealthCheck
    {
    }

}
