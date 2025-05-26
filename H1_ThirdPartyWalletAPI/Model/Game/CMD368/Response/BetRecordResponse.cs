using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{


    /// <summary>
    /// 注單查詢
    /// </summary>
    public class BetRecordResponse
    {
        /// <summary>
        /// 操作批次号
        /// </summary>
        public string SerialKey { get; set; }
        /// <summary>
        /// 操作批次时间
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 訊息說明
        /// </summary>
        public string Message { get; set; }

        public Daet[] Data { get; set; }

        //public Dictionary<long, int> LeagueId { get; set; }

        public class Daet
        {
            public DateTime TransDateFormatted => new DateTime(TransDate);
            public DateTime StateUpdateTsFormatted => new DateTime(StateUpdateTs);
            public Guid summary_id { get; set; }
            /// <summary>
            ///供查询的自增 ID
            /// </summary>
            public long Id { get; set; }
            /// <summary>
            /// 会员在合作商平台的标识
            /// </summary>
            public string SourceName { get; set; }
            /// <summary>
            /// 单号
            /// </summary>
            public string ReferenceNo { get; set; }
            /// <summary>
            /// 数据 ID
            /// </summary>
            public long SocTransId { get; set; }
            /// <summary>
            /// 是否是上半场
            /// </summary>
            public bool IsFirstHalf { get; set; }
            /// <summary>
            /// 下单时间,Ticks数据
            /// </summary>
            public long TransDate { get; set; }
            /// <summary>
            /// 是否为主队让球
            /// </summary>
            public bool IsHomeGive { get; set; }
            /// <summary>
            /// 是否投注主队
            /// </summary>
            public bool IsBetHome { get; set; }
            /// <summary>
            /// 下注金额
            /// </summary>
            public decimal BetAmount { get; set; }
            /// <summary>
            /// 用户未结算余额
            /// </summary>
            public float Outstanding { get; set; }
            /// <summary>
            /// 让球数
            /// </summary>
            public float Hdp { get; set; }
            /// <summary>
            /// 投注时的赔率
            /// </summary>
            public float Odds { get; set; }
            /// <summary>
            /// 货币代码
            /// </summary>
            public string Currency { get; set; }
            /// <summary>
            /// 输赢金额
            /// </summary>
            public decimal WinAmount { get; set; }
            /// <summary>
            /// 会员货币转换为马币的汇率
            /// </summary>
            public float ExchangeRate { get; set; }
            /// <summary>
            /// 输赢状态，WA = Win All，WH = Win Half，LA = Lose All，LH = Lose Half，D = Draw，P = Pending，P 为未结算其他为已经结算,CO = CashOut(自訂), RJ = Reject(自訂)
            /// </summary>
            public string WinLoseStatus { get; set; }
            /// <summary>
            /// 玩法类型1 1X2 下注 1 2 1X2 下注 2 CS 波胆 FLG 最先/最后进球 HDP 让球 HFT 半场/全场 OE 单/双 OU 大/小 OUT 优胜冠军 PAR 混合过关 TG 总进球 X 1X2 下注 X 1X 双重机会(DC) 下注 1X 12 双重机会(DC) 下注 12 X2 双重机会(DC) 下注 X2 ETG 准确总进球 HTG 主队准确总进球 ATG 客队准确总进球 HP3 三项让分投注 CNS 零失球
            /// </summary>
            public string TransType { get; set; }
            /// <summary>
            /// D: 正在处理的滚球赛事的单，N: 已接受今日或早盘的单 ，A: 已接受的滚球赛事的单，C: 已取消的单(一般为球赛取消造成)，R: 已拒绝的单
            /// </summary>
            public string DangerStatus { get; set; }
            /// <summary>
            /// 佣金设定值
            /// </summary>
            public float MemCommissionSet { get; set; }
            /// <summary>
            /// 会员所得佣金
            /// </summary>
            public float MemCommission { get; set; }
            /// <summary>
            /// 下注时的 IP
            /// </summary>
            public string BetIp { get; set; }
            /// <summary>
            /// 主队球队 ID
            /// </summary>
            public int HomeScore { get; set; }
            /// <summary>
            /// 客队球队 ID
            /// </summary>
            public int AwayScore { get; set; }
            /// <summary>
            /// 投注时主队得分
            /// </summary>
            public int RunHomeScore { get; set; }
            /// <summary>
            /// 投注时客队得分
            /// </summary>
            public int RunAwayScore { get; set; }
            /// <summary>
            /// 是否为滚球
            /// </summary>
            public bool IsRunning { get; set; }
            /// <summary>
            /// 拒绝理由
            /// </summary>
            public string RejectReason { get; set; }
            /// <summary>
            /// 球类标识
            /// </summary>
            public string SportType { get; set; }
            /// <summary>
            /// 投注位置
            /// </summary>
            public string Choice { get; set; }
            /// <summary>
            /// 所属做账日期
            /// </summary>
            public int WorkingDate { get; set; }
            /// <summary>
            /// 盘口类型，MY: 马来盘，ID: 印度尼西亚盘，HK: 香港盘，DE: 欧洲盘，US: 美国盘
            /// </summary>
            public string OddsType { get; set; }
            /// <summary>
            /// 球赛开赛日期
            /// </summary>
            public long MatchDate { get; set; }
            /// <summary>
            /// 主队球队 ID
            /// </summary>
            public int HomeTeamId { get; set; }
            /// <summary>
            /// 客队球队 ID
            /// </summary>
            public int AwayTeamId { get; set; }
            /// <summary>
            /// 联赛 ID
            /// </summary>
            public int LeagueId { get; set; }
            /// <summary>
            /// SpecialId 可透过 API，LanguageInfo 查询其特别投注名。SpecialId 可搭配中立场属性,若中立场属性有重复开启的状况,会出现以下变化:1000,15874(开立中立场)，15874(关闭中立场)，15874,1000(二次开启中立场)，15874,(二次关闭中立场)
            /// </summary>
            public string SpecialId { get; set; }
            /// <summary>
            /// true = 会员已经卖单
            /// </summary>
            public bool IsCashOut { get; set; }
            /// <summary>
            /// 如果会员已经卖单则为总卖单金额
            /// </summary>
            public decimal CashOutTotal { get; set; }
            /// <summary>
            /// 如果会员已经卖单则为卖单所得金额
            /// </summary>
            public decimal CashOutTakeBack { get; set; }
            /// <summary>
            /// 如果会员已经卖单则为会员卖单输赢
            /// </summary>
            public decimal CashOutWinLoseAmount { get; set; }
            /// <summary>
            /// 下注平台:1 = Desktop、7 = Mobile(v1)、9 = New Mobile(v1)、10 = Mobile E-Sport(v3)、11 = New Mobile E-Sport(v3)
            /// </summary>
            public int BetSource { get; set; }
            /// <summary>
            /// 是否重算标识(>=2 标识该单有重算)
            /// </summary>
            public int StatusChange { get; set; }
            /// <summary>
            /// 注单结算时间(如赛事结束后又重启赛事的情形,则注单结算时间以最后赛事结束时的注单结算时间为准。)
            /// </summary>
            public long StateUpdateTs { get; set; }
            /// <summary>
            /// 如果下注 AOS 则为不包括在 AOS 内的比分
            /// </summary>
            public string AOSExcluding { get; set; }
            /// <summary>
            /// MMK 币别专用,其他币别显示 0.0000
            /// </summary>
            public float MMRPercent { get; set; }
            /// <summary>
            /// 比赛 ID
            /// </summary>
            public int MatchID { get; set; }
            /// <summary>
            /// 比赛唯一标识符
            /// </summary>
            public string MatchGroupID { get; set; }
            /// <summary>
            /// OddsTrader 系统专用值. 若非 OddsTrader 传送来的值 , 即回传 String.Empty.
            /// </summary>
            public string BetRemarks { get; set; }
            /// <summary>
            /// 是否是特别投注
            /// </summary>
            public bool IsSpecial { get; set; }
            public decimal pre_betamount { get; set; }

            public decimal pre_winamount { get; set; }

            public string club_id { get; set; }

            public string franchiser_id { get; set; }

            /// <summary>
            /// 報表時間
            /// </summary>
            public DateTime report_time { get; set; }
            /// <summary>
            /// 分區時間
            /// </summary>
            public DateTime partition_time { get; set; }
            public decimal validbet { get; set; }

        }

    }
}
