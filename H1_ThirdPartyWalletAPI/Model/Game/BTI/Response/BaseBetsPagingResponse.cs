using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Response
{
    /// <summary>
    /// 回傳 注單共同欄位
    /// </summary>
    public class BaseBetsPagingResponse : BaseDataResponse
    {
        /// <summary>
        /// 下注內容 object
        /// </summary>
        public List<Bets> Bets { get; set; }

        /// <summary>
        /// yyyy-mm-dd hh:mm:ss 最后更新时间
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        /// <summary>
        /// 选择的页数
        /// </summary>
        public int currentPage { get; set; }

        /// <summary>
        /// 每页搜寻的比数 "totalPages"预设为 0, 此字段会显示当前选择的页数(假设当前没有数据则会回传 -1 )
        /// </summary>
        public int totalPages { get; set; }
    }

    /// <summary>
    /// 下注內容 object
    /// </summary>
    public class Bets
    {
        /// <summary>
        /// 结算时间 YYYY-MM-DD HH:MM:SS:ss
        /// </summary>
        public DateTime BetSettledDate { get; set; }

        /// <summary>
        /// 下注状态: 注意 “Declined” 拒绝是在系统有误才会有的。
        /// Opened 未结算
        /// Won 赢
        /// Lost 输
        /// Half Won 半赢
        /// Half Lost 半输
        /// Canceled 取消
        /// Cashout 提前兑现
        /// Draw 平局
        /// Declined 拒绝
        /// </summary>
        public string BetStatus { get; set; }

        /// <summary>
        /// 下注类别 ID
        /// 1 Single bets 单场投注
        /// 2 Combo bets 组合投注
        /// 3 System bet 系统投注
        /// 5 QA Bet QA 投注
        /// 6 Exact Score 准确比分
        /// 7 QA Bet QA 投注
        /// 13 System bet 系统投注
        /// </summary>
        public int BetTypeId { get; set; }

        /// <summary>
        /// 下注类别
        /// 1 Single bets 单场投注
        /// 2 Combo bets 组合投注
        /// 3 System bet 系统投注
        /// 5 QA Bet QA 投注
        /// 6 Exact Score 准确比分
        /// 7 QA Bet QA 投注
        /// 13 System bet 系统投注
        /// 
        /// 解释：QA 投注是问答形式的注单 常见的种类是 outright 赌冠军因为赌冠军的类型
        /// 是"请问以下队伍谁会夺冠" 答案会是一个字串:"A 队"所以我们把这种注单叫 QA 投注
        /// </summary>
        public string BetTypeName { get; set; }

        /// <summary>
        /// 代理名
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// 优惠金额不包括 PL 和 return
        /// </summary>
        public decimal ComboBonusAmount { get; set; }

        /// <summary>
        /// 下注时间 YYYY-MM-DD HH:MM:SS:ss
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// 货币 ISO 4217
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// BTi's 系统的资料库的玩家 ID.
        /// </summary>
        public int CustomerID { get; set; }

        /// <summary>
        /// 系统给代理的域名
        /// </summary>
        public int DomainID { get; set; }

        /// <summary>
        /// Freebet 资讯 Object
        /// </summary>
        public Freebet FreeBet { get; set; }

        /// <summary>
        /// 可赢额
        /// 未結算才有此欄位 可不用
        /// </summary>
        //public decimal Gain { get; set; }

        /// <summary>
        /// 玩家的 ID 唯一值(单一钱包和钱包接口使用的)(统一在系统的唯一值) 
        /// </summary>
        public string MerchantCustomerID { get; set; }

        /// <summary>
        /// 扣除了提前兑现剩下的值
        /// </summary>
        public decimal NonCashOutAmount { get; set; }

        /// <summary>
        /// Bet 数目
        /// </summary>
        public int NumberOfBets { get; set; }

        /// <summary>
        /// 美国盘口   文件是string 實際資料型態先用decimal
        /// </summary>
        public decimal Odds { get; set; }

        /// <summary>
        /// 欧洲盘赔率
        /// </summary>
        public decimal OddsDec { get; set; }

        /// <summary>
        /// 玩家看到的赔率
        /// </summary>
        public string OddsInUserStyle { get; set; }

        /// <summary>
        /// 玩家使用的盘口 比如 European 欧洲盘 / Malay 马来盘 / Hongkong 香港盘 / Indo 印尼盘 / American 美盘
        /// </summary>
        public string OddsStyleOfUser { get; set; }

        /// <summary>
        /// 净利（扣除了下注额）
        /// </summary>
        public decimal PL { get; set; }

        /// <summary>
        /// 平台: Web 电脑网站下注/ Mobile 手机网页下注
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// 玩家等级 ID
        /// 0 是新玩家
        /// 1 是普通玩家
        /// 10 是测试玩家
        /// </summary>
        public int PlayerLevelID { get; set; }

        /// <summary>
        /// 玩家等级名
        /// </summary>
        public string PlayerLevelName { get; set; }

        /// <summary>
        /// 注单购买 ID
        /// </summary>
        public string PurchaseID { get; set; }

        /// <summary>
        /// Actual amount after freebet amount is striped
        /// </summary>
        public decimal RealMoneyAmount { get; set; }

        /// <summary>
        /// 玩家获取的数目
        /// </summary>
        public decimal Return { get; set; }

        /// <summary>
        /// 数据开始创建日期
        /// </summary>
        public DateTime SearchDateTime { get; set; }

        /// <summary>
        /// Selections – 赌注选项 Array
        /// </summary>
        public List<Selections> Selections { get; set; }

        /// <summary>
        /// Status – 注单状态（新的是 BetStatus 取代旧的 Status，请用新的 BetStatus, 百家乐不取这个值）
        /// </summary>
        //public string Status { get; set; }

        /// <summary>
        /// 串关的类型 比如 3 串 2 系统2/3
        /// 3 串 1 就是 combo 3 场全赢才派彩，3 串 2 赢 2/3 就代表三场中任两场 就派部分彩金
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal TotalStake { get; set; }

        /// <summary>
        /// 更新时间 YYYY-MM-DD HH:MM:SS:ss
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// 玩家的账号名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 正式下注额 （如果是提前兑现是永远是0）
        /// </summary>
        public decimal ValidStake { get; set; }

        /// <summary>
        /// Guid
        /// </summary>
        public Guid summary_id { get; set; }

        /// <summary>
        /// [原始]下注金额
        /// </summary>
        public decimal? pre_TotalStake { get; set; }

        /// <summary>
        /// [原始]正式下注额
        /// </summary>
        public decimal? pre_ValidStake { get; set; }

        /// <summary>
        /// [原始]净利（扣除了下注额）
        /// </summary>
        public decimal? pre_PL { get; set; }

        /// <summary>
        /// [原始]玩家获取的数目
        /// </summary>
        public decimal? pre_Return { get; set; }

        /// <summary>
        /// [原始]下注状态: 注意 “Declined” 拒绝是在系统有误才会有的。
        /// Opened 未结算
        /// Won 赢
        /// Lost 输
        /// Half Won 半赢
        /// Half Lost 半输
        /// Canceled 取消
        /// Cashout 提前兑现
        /// Draw 平局
        /// Declined 拒绝
        /// </summary>
        public string pre_BetStatus { get; set; }

        /// <summary>
        /// BranchID - 体育类别 ID 
        /// </summary>
        public int BranchID { get; set; }

        /// <summary>
        /// BranchName – 体育类型名
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// LeagueName 联赛名
        /// </summary>
        public string LeagueName { get; set; }

        /// <summary>
        /// HomeTeam 主队名
        /// </summary>
        public string HomeTeam { get; set; }

        /// <summary>
        /// AwayTeam 客队名
        /// </summary>
        public string AwayTeam { get; set; }

        /// <summary>
        /// YourBet – 玩家下注选项, 如果是百家乐，请看附录 4.2.1
        /// </summary>
        public string YourBet { get; set; }

        public string club_id { get; set; }

        public string franchiser_id { get; set; }
    }

    /// <summary>
    /// Freebet 资讯 Object
    /// </summary>
    public class Freebet
    {
        /// <summary>
        /// Freebet 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 注单是否为 RiskFreebet
        /// </summary>
        public int IsRiskFreeBet { get; set; }
    }

    /// <summary>
    /// Selections – 赌注选项 Array
    /// </summary>
    public class Selections
    {
        /// <summary>
        /// ActionType – 结算类型 (freebet 免费下注, RiskFreebet 无风险下注, Real 下注, Cashout 提前兑现)
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// AwayTeam 客队名
        /// </summary>
        public string AwayTeam { get; set; }

        /// <summary>
        /// BetID 下注 ID 文件是Int64 實際抓到資料是string
        /// </summary>
        public string BetID { get; set; }

        /// <summary>
        /// BetType – 下注类型
        /// </summary>
        public string BetType { get; set; }

        /// <summary>
        /// BonusID – 与优惠卷有关的
        /// </summary>
        public int BonusID { get; set; }

        /// <summary>
        /// BranchID 体育类别 ID
        /// </summary>
        public int BranchID { get; set; }

        /// <summary>
        /// BranchName – 体育类型名
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// CouponCode BTi 系统优惠券码
        /// </summary>
        public string CouponCode { get; set; }

        /// <summary>
        /// 不再使用
        /// </summary>
        //public string EncodedLineID { get; set; }
        
        /// <summary>
        /// YYYY-MM-DD HH:MM:SS:ss 赛事日期
        /// </summary>
        public DateTime EventDate { get; set; }

        /// <summary>
        /// 赛事类型 ID
        /// </summary>
        public Int64 EventTypeID { get; set; }

        /// <summary>
        /// Event type name 下注的赛名
        /// </summary>
        public string EventTypeName { get; set; }

        /// <summary>
        /// 游戏 ID
        /// 收到值是 457122686041407488 轉收後變 457122686041407500
        /// 收到值是 457122659923488768 轉收後變 457122659923488800
        /// todo: 要在確認型態
        /// </summary>
        public UInt64 GameID { get; set; }

        /// <summary>
        /// HomeTeam 主队名
        /// </summary>
        public string HomeTeam { get; set; }

        /// <summary>
        /// 提前派彩– 1 是 0 不是
        /// </summary>
        public int IsEarlyPayout { get; set; }

        /// <summary>
        /// 滚地赛事 是 1，不是是 0
        /// </summary>
        public int IsLive { get; set; }

        /// <summary>
        /// 滚地赛事是否有变 1 是，0 不是
        /// </summary>
        public int IsNewLine { get; set; }

        /// <summary>
        /// 联赛 ID 
        /// todo: 
        /// </summary>
        public UInt64 LeagueID { get; set; }

        /// <summary>
        /// LeagueName 联赛名
        /// </summary>
        public string LeagueName { get; set; }

        /// <summary>
        /// LineID 永远是 0 
        /// </summary>
        public int LineID { get; set; }

        /// <summary>
        /// 玩法类型 ID
        /// </summary>
        public Int64 LineTypeID { get; set; }

        /// <summary>
        /// 玩法类型名
        /// </summary>
        public string LineTypeName { get; set; }

        /// <summary>
        /// 当下滚地下注的主队比分，如果是滚地会有
        /// 文件是string 實際抓到資料是int
        /// </summary>
        public int LiveScore1 { get; set; }

        /// <summary>
        /// 当下滚地下注的客队比分，如果是滚地会有
        /// 文件是string 實際抓到資料是int
        /// </summary>
        public int LiveScore2 { get; set; }

        /// <summary>
        /// Odds 美国盘口
        /// </summary>
        public int Odds { get; set; }

        /// <summary>
        /// OddsDec 欧洲盘赔率
        /// </summary>
        public decimal OddsDec { get; set; }

        /// <summary>
        /// OddsInUserStyle 玩家看到的赔率
        /// </summary>
        public string OddsInUserStyle { get; set; }

        /// <summary>
        /// Points – 球头，让分
        /// </summary>
        public decimal Points { get; set; }

        /// <summary>
        /// 代理商用来对应优惠券的 ID
        /// </summary>
        public int ReferenceID { get; set; }

        /// <summary>
        /// RelatedBetID 提前兑现有关的注单 ID
        /// </summary>
        public Int64 RelatedBetID { get; set; }

        /// <summary>
        /// Score – 最后比分，只有是与比分有关才有, 如果是百家乐，请看附录 4.2.2 
        /// </summary>
        public string Score { get; set; }

        /// <summary>
        /// Status – 注单状态（新的是 BetStatus 取代旧的 Status，请用新的 BetStatus, 百家乐不取这个值）
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// YourBet – 玩家下注选项, 如果是百家乐，请看附录 4.2.1
        /// </summary>
        public string YourBet { get; set; }

        /// <summary>
        /// 免费下注– 1 是 0 不是
        /// </summary>
        public int isfreebet { get; set; }

        /// <summary>
        /// isresettled (弃用)– 是重新结算 1 是 0 不是
        /// </summary>
        //public int isresettled { get; set; }
    }
}
